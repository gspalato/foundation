using System;
using System.Collections.Generic;
using ByteDev.DotNet.Project;

namespace Foundation.Tools.Codegen.Structures;

public class Project
{
    public required string Name { get; set; }

    public required string Path { get; set; }

    public required IEnumerable<TargetFramework> Frameworks { get; set; }

    public Dictionary<Guid, SourceFile> Files { get; set; } = new();
}
