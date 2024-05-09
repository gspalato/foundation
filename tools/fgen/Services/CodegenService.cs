using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Foundation.Tools.Codegen.Generators;
using Foundation.Tools.Codegen.Structures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Spectre.Console;

namespace Foundation.Tools.Codegen.Services;

public partial class CodegenService
{

    private const string GeneratedFolderName = "generated";


    private IOService IOService { get; }

    
    private CSharpCompilation GlobalCompilation = CSharpCompilation.Create("Foundation");

    private List<IGenerationPipeline> Pipelines { get; } = new();

    private List<Project> Projects { get; } = new();

    public CodegenService(IOService ioService)
    {
        IOService = ioService;

        RegisterPipelines();
    }

    /// <summary>
    ///  Registers all pipelines that should be used for code generation.
    ///  This is where you can add your own custom pipelines.
    /// </summary>
    /// <remarks>
    /// Pipelines are used to specify which generators should run on which files.
    /// </remarks>
    private void RegisterPipelines()
    {
        // This pipeline is used to generate a QueryType class from a base Query class.
        // It's used to extract metrics from each method, and adds an introspection query.
        var queryPipeline = new GenerationPipelineBuilder()
            .WithName("Query")
            .AddGenerator<IntrospectionQueryGenerator>()
            .AddGenerator<QueryTypeGenerator>()
            .AddGenerator<InjectDatabaseMetricsGenerator>()
            .Build();

        Pipelines.Add(queryPipeline);
    }

    /// <summary>
    ///   Loads a project into the service.
    /// </summary>
    /// <param name="project"></param>
    public void LoadProject(Project project)
    {
        Projects.Add(project);

        var trees = GetAllSyntaxTreesFromProject(project);
        GlobalCompilation = GlobalCompilation.AddSyntaxTrees(trees.Select(t => t.Value));
    }

    /// <summary>
    ///   Loads multiple projects into the service.
    /// </summary>
    /// <param name="projects"></param>
    public void LoadProjects(IEnumerable<Project> projects)
    {
        Projects.AddRange(projects);

        var trees = projects.Select(p => GetAllSyntaxTreesFromProject(p).Select(t => t.Value));
        GlobalCompilation = GlobalCompilation.AddSyntaxTrees(trees.SelectMany(t => t));
    }

    /// <summary>
    ///   Gets all syntax trees from a project.
    /// </summary>
    /// <param name="project">The project to get syntax trees from.</param>
    /// <returns>A dictionary of syntax trees, where the key is the file's GUID.</returns>
    public static Dictionary<Guid, SyntaxTree> GetAllSyntaxTreesFromProject(Project project)
    {
        var trees = new Dictionary<Guid, SyntaxTree>();
        foreach (var file in project.Files)
        {
            var tree = CSharpSyntaxTree.ParseText(file.Value.Content);
            trees.Add(file.Key, tree);
        }

        return trees;
    }

    /// <summary>
    ///   Generates code for a project.
    ///   Runs all generators on all files in the project.
    ///   Then saves the generated code to the project's obj/foundation_generated folder.
    /// </summary>
    /// <param name="project"></param>
    public void GenerateForProject(Project project)
    {
        IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        Dictionary<Guid, SyntaxTree> trees = GetAllSyntaxTreesFromProject(project);

        // Each tree is a parsed file in the project.
        foreach (var tree in trees)
        {
            // Get the file that corresponds to the tree.
            var file = project.Files[tree.Key];
            var expectedFilename = Path.GetFileNameWithoutExtension(file.Name);
            var originalSyntaxTree = tree.Value;

            // Set initial values.
            var syntaxTree = tree.Value;
            var source = file.Content;

            var allTreePipelines = new List<IGenerationPipeline>();

            // Each definition is a collection of classes and their generation pipelines.
            bool wasModified = false;
            foreach (var node in tree.Value.GetRoot().DescendantNodes())
            {
                // Warning! This is a hacky way to get the class node.
                // For now, only classes are supported.
                // I plan to add method support in the future.
                if (node is not ClassDeclarationSyntax @class)
                    continue;

                // Check if node (class, method, etc.) has a generation comment.
                var definition = ParseGenerationComment(node);
                if (definition is null)
                    continue;

                // Get class and pipeline lists for each tree/file.
                var attributed = definition.AttributedPipelines;
                if (attributed.Count == 0)
                    continue;

                allTreePipelines.AddRange(attributed);


                foreach (var pipeline in attributed)
                {
                    AnsiConsole.MarkupLineInterpolated(
                        $"{Emoji.Known.Gear}  [bold blue]Running pipeline [aqua]{pipeline.Name}[/] on [aqua]{@class.Identifier}[/]...[/]"
                    );

                    var pipelineExecutionId = Guid.NewGuid().ToString();

                    // Add a SyntaxAnnotation to the class node with the pipeline execution ID.
                    // This is used to identify the node later, after it's been modified.
                    var currentPipelineAnnotation = new SyntaxAnnotation(pipelineExecutionId);
                    var annotatedClass = @class.WithAdditionalAnnotations(currentPipelineAnnotation);
                    syntaxTree = syntaxTree.GetCompilationUnitRoot().ReplaceNode(@class, annotatedClass).NormalizeWhitespace().SyntaxTree;
                    @class = annotatedClass;

                    foreach (var generatorType in pipeline.GeneratorTypes)
                    {
                        var generator = (IGenerator)Activator.CreateInstance(generatorType)!;

                        // Import projects that this generator depends on.
                        //var dependencies = generator.Import(Projects);
                        //foreach (var dep in dependencies)
                        //    GlobalCompilation = GlobalCompilation.AddSyntaxTrees(GetAllSyntaxTreesFromProject(dep).Values);

                        // Setup generator with variables and compilation with the latest syntax tree.
                        generator.Setup(cache, GlobalCompilation, currentPipelineAnnotation, project, file, syntaxTree, @class);

                        foreach (var visitNode in tree.Value.GetRoot().DescendantNodes())
                            generator.OnVisitSyntaxNode(visitNode);

                        var result = generator.Generate();
                        if (!result.Success)
                            break;

                        wasModified = true;

                        syntaxTree = generator.GetSyntaxTree();

                        @class = (ClassDeclarationSyntax)syntaxTree
                            .GetCompilationUnitRoot()
                            .DescendantNodes()
                            .FirstOrDefault(c => c.HasAnnotation(currentPipelineAnnotation))!;

                        expectedFilename = result.ExpectedFilename ?? expectedFilename;
                    }
                }

                // Add Generated attribute to class (from Foundation.Common).
                var pipelineArguments = attributed.Select(p => SyntaxFactory
                    .AttributeArgument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(p.Name)
                        )
                    )
                );

                var generatedAttribute = SyntaxFactory
                    .Attribute(SyntaxFactory.IdentifierName("Foundation.Common.Generated"))
                    .AddArgumentListArguments(pipelineArguments.ToArray());

                var generatedAttributeList = SyntaxFactory.AttributeList().AddAttributes(generatedAttribute);

                syntaxTree = syntaxTree
                    .GetCompilationUnitRoot()
                    .ReplaceNode(@class, @class.AddAttributeLists(generatedAttributeList))
                    .NormalizeWhitespace()
                    .SyntaxTree;
            }

            if (!wasModified)
                continue;

            source = syntaxTree.GetRoot().NormalizeWhitespace().ToFullString();

            var resultSource = AddAutogeneratedComments(source, allTreePipelines);

            var filename = $"{expectedFilename}.fndtn.g.cs";

            AnsiConsole.MarkupLineInterpolated(
                $"{Emoji.Known.CheckMarkButton} [bold green]Generated [aqua]{filename}[/] [green]from project[/] [aqua]{project.Name}[/] [green]at[/][/] [dim]{Utilities.CollapsePath(file.Path)}.[/]"
            );

            // Create a new file in the project's Generated folder.
            var path = Path.Combine(project.Path, "Generated");
            IOService.WriteFile(filename, path, resultSource);

            AnsiConsole.MarkupLineInterpolated(
                $"{Emoji.Known.FloppyDisk} [bold green]Saved [aqua]{filename}[/] [green]at[/][/] [dim]{Utilities.CollapsePath(path)}.[/]"
            );
        }
    }

    /// <summary>
    ///   Adds autogenerated comments to the top of the source code.
    /// </summary>
    /// <param name="source">The source code to add comments to.</param>
    /// <param name="pipelines">The pipelines that were executed.</param>
    /// <returns>The source code with comments added.</returns>
    private static string AddAutogeneratedComments(string source, List<IGenerationPipeline> pipelines)
    {
        var sourceBuilder = new StringBuilder();

        var pipelineList = string.Join(", ", pipelines.Distinct().Select(p => p.Name));

        sourceBuilder.AppendLine($"""
        // <auto-generated>
        // ================================= FOUNDATION =================================
        // This file was autogenerated by Foundation.
        // Generation pipelines applied: {pipelineList}.
        //
        // Instructions:
        // • Do not modify this file directly, as it will be overwritten.
        // • Do not check this file into source control.
        // • To regenerate or update this file, run the *fgen* tool.
        // ==============================================================================
        // </auto-generated>

        """);

        sourceBuilder.Append(source);

        return sourceBuilder.ToString();
    }

    /// <summary>
    ///   Parses the node for generation comments.
    /// </summary>
    /// <param name="tree">The syntax tree to parse.</param>
    /// <returns>A GenerationDefinition including the pipelines that should be executed, in order.</returns>
    /// <remarks>
    ///   Generation comments are comments that start with <c>// foundation generate </c>.
    ///   They are used to specify which pipelines should run on a node.
    ///   Multiple pipelines can be specified, separated by commas.
    /// </remarks>
    private GenerationDefinition? ParseGenerationComment(SyntaxNode node)
    {
        /* Example:
         *
         * // foundation generate query
         * public class MyClass { }
         *
         */
        Regex commentRegex = GenerateCommentRegex();

        var trivia = node.GetLeadingTrivia();
        var comments = trivia.Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia));

        IEnumerable<string> pipelineNames;
        try
        {
            pipelineNames =
                from comment in comments
                where commentRegex.IsMatch(comment.ToFullString())
                from match in commentRegex.Matches(comment.ToFullString())
                from name in match.Value.Split(',')
                select name.Trim();
        }
        catch
        {
            return null;
        }

        var pipelines = Pipelines.Where(p => pipelineNames.Contains(p.Name.ToLowerInvariant())).ToList();

        if (pipelines.Count == 0)
            return null;

        return new GenerationDefinition
        {
            SyntaxTree = node.SyntaxTree,
            TargetNode = node,
            AttributedPipelines = pipelines
        };
    }

    [GeneratedRegex("(?<=\\/{2,}\\s*foundation generate )(.*)+")]
    private static partial Regex GenerateCommentRegex();
}