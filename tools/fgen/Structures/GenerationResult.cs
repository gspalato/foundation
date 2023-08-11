using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Tools.Codegen.Structures;

public class GenerationResult
{
    public bool Success { get; set; }

    public string ExpectedFilename { get; set; } = default!;

    public SyntaxNode Node = default!;
}