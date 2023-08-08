using System;
using System.Collections.Generic;
using Foundation.Tools.Codegen.Structures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Tools.Codegen.Generators;

public interface IGenerator
{
    GenerationResult Generate(SyntaxTree syntaxTree, SourceFile sourceFile, Project project);
    void OnVisitSyntaxNode(SyntaxNode node);
}

public abstract class Generator : IGenerator
{
    public abstract GenerationResult Generate(SyntaxTree syntaxTree, SourceFile sourceFile, Project project);

    public abstract void OnVisitSyntaxNode(SyntaxNode syntaxNode);
}