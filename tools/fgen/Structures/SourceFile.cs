using System.Linq;

namespace Foundation.Tools.Codegen
{
    public class SourceFile
    {
        public string Name => Path.Split('/').Last();
        public string Path { get; set; }
        public string Content { get; set; }
    }
}