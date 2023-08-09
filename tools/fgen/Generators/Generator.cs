using System.Collections.Generic;
using Foundation.Tools.Codegen.Structures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Caching.Memory;

namespace Foundation.Tools.Codegen.Generators;

public interface IGenerator
{
    GenerationResult Generate();
    void OnVisitSyntaxNode(SyntaxNode node);
}

public abstract class Generator : IGenerator
{
    protected IMemoryCache Cache { get; set; }

    protected CSharpCompilation Compilation { get; set; }

    protected Dictionary<SyntaxNode, string[]> GenerationOptions { get; set; }

    protected Project Project { get; set; }

    protected SemanticModel SemanticModel => Compilation.GetSemanticModel(SyntaxTree);

    protected SourceFile SourceFile { get; set; }

    protected SyntaxTree SyntaxTree { get; set; }

    public void Setup(
        IMemoryCache cache,
        CSharpCompilation compilation,
        Dictionary<SyntaxNode, string[]> generationOptions,
        Project project,
        SourceFile sourceFile,
        SyntaxTree syntaxTree
    )
    {
        Cache = cache;
        Compilation = compilation;
        GenerationOptions = generationOptions;
        Project = project;
        SourceFile = sourceFile;
        SyntaxTree = syntaxTree;
    }

    public abstract GenerationResult Generate();

    public abstract void OnVisitSyntaxNode(SyntaxNode syntaxNode);
}