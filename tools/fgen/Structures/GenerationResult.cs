using Microsoft.CodeAnalysis;

namespace Foundation.Tools.Codegen.Structures;

public class GenerationResult
{
    public bool Success { get; set; }
    
    public string Source { get; set; }

    public string ExpectedFilename { get; set; }

    public SyntaxTree SyntaxTree { get; set; }
}