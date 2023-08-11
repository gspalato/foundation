using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Foundation.Tools.Codegen.Generators;
using Foundation.Tools.Codegen.Structures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;

namespace Foundation.Tools.Codegen.Services;

public class CodegenService
{
    private IOService IOService { get; }


    private const string GeneratedFolderName = "fgenerated";

    private List<IGenerationPipeline> Pipelines { get; } = new();

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
            .Build();

        Pipelines.Add(queryPipeline);
    }

    /// <summary>
    ///  Gets all syntax trees from a project.
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
        var compilation = CSharpCompilation.Create(project.Name)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        Dictionary<Guid, SyntaxTree> trees = GetAllSyntaxTreesFromProject(project);

        foreach (var tree in trees)
            compilation = compilation.AddSyntaxTrees(tree.Value);

        // Each tree is a parsed file in the project.
        foreach (var tree in trees)
        {
            // Get the file that corresponds to the tree.
            var file = project.Files[tree.Key];
            var expectedFilename = Path.GetFileNameWithoutExtension(file.Name);

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
                    Console.WriteLine($"Running pipeline {pipeline.Name}.");
                    var pipelineExecutionId = Guid.NewGuid().ToString();

                    foreach (var generatorType in pipeline.GeneratorTypes)
                    {
                        var generator = (IGenerator)Activator.CreateInstance(generatorType)!;
                        generator.Setup(cache, compilation, pipelineExecutionId, project, file, syntaxTree, @class);

                        foreach (var visitNode in tree.Value.GetRoot().DescendantNodes())
                            generator.OnVisitSyntaxNode(visitNode);

                        Console.WriteLine($"Running generator {generatorType.Name} on {@class.Identifier} in pipeline {pipeline.Name}");

                        var result = generator.Generate();
                        if (!result.Success)
                            break;

                        wasModified = true;

                        syntaxTree = result.Node.SyntaxTree;
                        expectedFilename = result.ExpectedFilename ?? expectedFilename;

                        // Redefine class as the result from the last generator.
                        @class = result.Node as ClassDeclarationSyntax ?? @class;
                    }
                }
            }

            if (!wasModified)
                continue;
            
            source = syntaxTree.GetRoot().NormalizeWhitespace().ToFullString();

            var resultSource = AddAutogeneratedComments(FormatCode(source), allTreePipelines);

            // Create a new file in the obj/Debug and obj/Release folders for each target framework.
            static string GetPathForTarget(string path, string type, string target)
                => Path.Combine(path, "obj", type, target, GeneratedFolderName);

            var filename = $"{expectedFilename}.g.cs";
            var generatedPath = GetPathForTarget(project.Path, "[Type]", "[TargetFramework]");
            foreach (var target in project.Frameworks)
            {
                var debugPath = GetPathForTarget(project.Path, "Debug", target.Moniker);
                var releasePath = GetPathForTarget(project.Path, "Release", target.Moniker);

                IOService.WriteFile(filename, debugPath, resultSource);
                IOService.WriteFile(filename, releasePath, resultSource);
            }

            Console.WriteLine($"Created file {filename} in {generatedPath}.");
        }
    }

    /// <summary>
    ///  Adds autogenerated comments to the top of the source code.
    /// </summary>
    /// <param name="source">The source code to add comments to.</param>
    /// <param name="pipelines">The pipelines that were executed.</param>
    /// <returns>The source code with comments added.</returns>
    private static string AddAutogeneratedComments(string source, List<IGenerationPipeline> pipelines)
    {
        var sourceBuilder = new StringBuilder();

        var pipelineList = string.Join(", ", pipelines.Distinct().Select(p => p.Name));

        sourceBuilder.AppendLine($"// <auto-generated>                                                              ");
        sourceBuilder.AppendLine($"// ================================= FOUNDATION =================================");
        sourceBuilder.AppendLine($"// This file was autogenerated by Foundation.                                    ");
        sourceBuilder.AppendLine($"// Generation pipelines applied: {pipelineList}.                                 ");
        sourceBuilder.AppendLine($"//                                                                               ");
        sourceBuilder.AppendLine($"// Instructions:                                                                 ");
        sourceBuilder.AppendLine($"// * Do not modify this file directly, as it will be overwritten.                ");
        sourceBuilder.AppendLine($"// * Do not check this file into source control.                                 ");
        sourceBuilder.AppendLine($"// * To regenerate this file, run the *fgen* tool.                               ");
        sourceBuilder.AppendLine($"// ==============================================================================");
        sourceBuilder.AppendLine($"// </auto-generated>                                                             ");
        sourceBuilder.AppendLine();
        sourceBuilder.Append(source);

        return sourceBuilder.ToString();
    }

    /// <summary>
    ///  Parses the node for generation comments.
    /// </summary>
    /// <param name="tree">The syntax tree to parse.</param>
    /// <returns>A GenerationDefinition including the pipelines that should be executed, in order.</returns>
    /// <remarks>
    ///  Generation comments are comments that start with <c>// foundation generate </c>.
    ///  They are used to specify which generators should run on a class.
    /// </remarks>
    private GenerationDefinition? ParseGenerationComment(SyntaxNode node)
    {
        const string generateComment = "// foundation generate ";

        var trivia = node.GetLeadingTrivia();
        var comments = trivia.Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia));

        string foundGenerateComment = default!;
        try
        {
            foundGenerateComment = comments
                .Select(c => c.ToFullString())
                .First(c => c.StartsWith(generateComment));
        }
        catch
        {
            return null;
        }

        var pipelineNames = foundGenerateComment[generateComment.Length..]
            .Split(',')
            .Select(s => s.Trim())
            .ToArray();

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

    /// <summary>
    ///  Formats the code to be more human readable.
    /// </summary>
    /// <param name="code">The code to format.</param>
    /// <param name="cancelToken">The cancellation token.</param>
    /// <returns>The formatted code.</returns>
    /// <remarks>
    ///  Even though is code is autogenerated and will only be ran by the compiler,
    ///  it's still nice to have it formatted properly. This is especially useful for debugging.
    /// </remarks>
    private static string FormatCode(string code, CancellationToken cancelToken = default)
    {
        return CSharpSyntaxTree.ParseText(code, cancellationToken: cancelToken)
            .GetRoot(cancelToken)
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText(cancelToken)
            .ToString();
    }
}