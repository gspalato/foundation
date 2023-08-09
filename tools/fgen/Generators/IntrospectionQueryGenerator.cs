using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Foundation.Tools.Codegen.Structures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;

namespace Foundation.Tools.Codegen.Generators;

public class IntrospectionQueryGenerator : Generator
{
    public IntrospectionQueryGenerator(IMemoryCache cache, CSharpCompilation compilation, Project project, SourceFile sourceFile, SyntaxTree syntaxTree)
    {
        Cache = cache;
        Compilation = compilation;
        Project = project;
        SourceFile = sourceFile;
        SyntaxTree = syntaxTree;
    }

    public List<SyntaxNode> CandidateNamespaces { get; } = new();
    public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

    /// <summary>
    /// Modifies a Query class to include a new method that allows for introspection.
    /// </summary>
    /// <param name="syntaxTree"></param>
    /// <param name="sourceFile"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    public override GenerationResult Generate()
    {
        // If there are no classes with the [Query] attribute, then return an empty result.
        if (CandidateClasses.Count == 0)
            return new GenerationResult()
            {
                Success = false,
                Source = ""
            };

        // Select only one class, as they will be merged in a later generation step.
        Cache.TryGetValue("Query_InjectedInstrospection", out bool alreadyInjected);
        if (alreadyInjected)
            return new GenerationResult()
            {
                Success = false,
                Source = ""
            };

        var @class = CandidateClasses.First();
        Cache.CreateEntry("Query_InjectedInstrospection").SetValue(true);

        // Create new method called "GetIntrospectionInfoAsync" using SyntaxFactory.
        StatementSyntax[] statements = new[]
        {
            SyntaxFactory.ParseStatement("return Task.FromResult(\"Introspection info\");")
        };

        var method = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName("Task<string>"), "GetIntrospectionInfoAsync")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
            .AddParameterListParameters()
            .AddBodyStatements(statements);

        var modifiedClass = @class.AddMembers(method);

        // Generate source code.
        // Note: This was the best working solution yet.
        //       Directly using the SyntaxFactory to create the source code was not working.
        //       It didn't add spaces between keywords properly.
        var source = SyntaxTree.GetRoot().ReplaceNode(@class, modifiedClass).NormalizeWhitespace().ToFullString();

        return new GenerationResult()
        {
            Success = true,
            Source = source,
            ExpectedFilename = Path.GetFileNameWithoutExtension(SourceFile.Name),
        };
    }

    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
            && HasGenerateAttribute(classDeclarationSyntax)
            && IsQueryClass(classDeclarationSyntax)
        )
            CandidateClasses.Add(classDeclarationSyntax);
    }

    private static bool HasGenerateAttribute(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return classDeclarationSyntax.AttributeLists
            .SelectMany(attributeList => attributeList.Attributes)
            .Any(attribute => attribute.Name.ToString() is "Generate");
    }

    private static bool IsQueryClass(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return classDeclarationSyntax.Identifier.Text is "Query";
    }
}