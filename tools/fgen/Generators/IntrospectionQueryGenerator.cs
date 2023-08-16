using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Foundation.Tools.Codegen.Structures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;

namespace Foundation.Tools.Codegen.Generators;

public class IntrospectionQueryGenerator : Generator
{
    /// <summary>
    /// Modifies a Query class to include a new method that allows for introspection.
    /// </summary>
    /// <param name="syntaxTree"></param>
    /// <param name="sourceFile"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    public override GenerationResult Generate()
    {
        if (Target is null)
            return new GenerationResult()
            {
                Success = false
            };

        var @class = (ClassDeclarationSyntax)GetTarget();

        // Create new method called "GetIntrospectionInfoAsync" using SyntaxFactory.
        StatementSyntax[] statements = new[]
        {
            SyntaxFactory.ParseStatement("return Task.FromResult(\"Introspection info\");")
        };

        var method = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName("Task<string>"), "GetIntrospectionInfoAsync")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
            .AddParameterListParameters()
            .AddBodyStatements(statements);

        @class = @class.AddMembers(method);

        SyntaxTree = SyntaxTree.GetCompilationUnitRoot().ReplaceNode(GetTarget(), @class).SyntaxTree;

        return new GenerationResult()
        {
            Success = true,
            Node = @class,
            ExpectedFilename = Path.GetFileNameWithoutExtension(SourceFile.Name),
        };
    }

    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode) { }
}