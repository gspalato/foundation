using System;
using System.Collections.Generic;
using System.Linq;
using Foundation.Tools.Codegen.Structures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;

namespace Foundation.Tools.Codegen.Generators;

public interface IGenerator
{
    GenerationResult Generate();

    void OnVisitSyntaxNode(SyntaxNode node);

    void Setup(
        IMemoryCache cache,
        CSharpCompilation compilation,
        string pipelineExecutionId,
        Project project,
        SourceFile sourceFile,
        SyntaxTree syntaxTree,
        ClassDeclarationSyntax target
    );
}

public abstract class Generator : IGenerator
{
    protected IMemoryCache Cache { get; set; } = default!;

    protected CSharpCompilation Compilation { get; set; } = default!;

    protected string PipelineExecutionId { get; set; } = default!;

    protected Project Project { get; set; } = default!;

    protected SemanticModel SemanticModel => Compilation.GetSemanticModel(SyntaxTree);

    protected SourceFile SourceFile { get; set; } = default!;

    protected SyntaxTree SyntaxTree { get; set; } = default!;

    protected ClassDeclarationSyntax Target { get; set; } = default!;

    public void Setup(
        IMemoryCache cache,
        CSharpCompilation compilation,
        string pipelineExecutionId,
        Project project,
        SourceFile sourceFile,
        SyntaxTree syntaxTree,
        ClassDeclarationSyntax target
    )
    {
        Cache = cache;
        Compilation = compilation;
        PipelineExecutionId = pipelineExecutionId;
        Project = project;
        SourceFile = sourceFile;
        SyntaxTree = syntaxTree;
        Target = target;
    }

    public abstract GenerationResult Generate();

    public abstract void OnVisitSyntaxNode(SyntaxNode syntaxNode);
}