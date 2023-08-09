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

    protected Project Project { get; set; }

    protected SemanticModel SemanticModel => Compilation.GetSemanticModel(SyntaxTree);

    protected SourceFile SourceFile { get; set; }

    protected SyntaxTree SyntaxTree { get; set; }

    public abstract GenerationResult Generate();

    public abstract void OnVisitSyntaxNode(SyntaxNode syntaxNode);
}