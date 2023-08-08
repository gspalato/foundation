using System;
using System.Collections.Generic;
using ByteDev.DotNet.Project;

namespace Foundation.Tools.Codegen.Structures;

public class Project
{
    public string Name { get; set; }

    public string Path { get; set; }

    public IEnumerable<TargetFramework> Frameworks { get; set; }

    public Dictionary<Guid, SourceFile> Files { get; set; } = new();
}
