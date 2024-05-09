using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Tools.Codegen.Structures;

public class GenerationDefinition
{
    public SyntaxTree SyntaxTree { get; set; } = default!;

    public SyntaxNode TargetNode { get; set; } = default!;

    public List<IGenerationPipeline> AttributedPipelines { get; set; } = new();
}