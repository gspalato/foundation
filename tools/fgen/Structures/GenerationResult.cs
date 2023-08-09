using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Foundation.Tools.Codegen.Structures;

public class GenerationResult
{
    public bool Success { get; set; }
    
    public string Source { get; set; }

    public string ExpectedFilename { get; set; }

    public SyntaxTree SyntaxTree => CSharpSyntaxTree.ParseText(Source);
}