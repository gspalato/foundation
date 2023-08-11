using System.Linq;

namespace Foundation.Tools.Codegen.Structures;

public class SourceFile
{
    public string Name => Path.Split('/').Last();
    public required string Path { get; set; }
    public required string Content { get; set; }
}
