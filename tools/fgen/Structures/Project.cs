using System;
using System.Collections.Generic;

namespace Foundation.Tools.Codegen.Structures;

public class Project
{
    public string Name { get; set; }
    public string Path { get; set; }
    public Dictionary<Guid, SourceFile> Files { get; set; } = new();
}
