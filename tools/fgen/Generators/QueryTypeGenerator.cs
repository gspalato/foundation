using System;
using System.Collections.Generic;
using System.Linq;
using Foundation.Tools.Codegen.Structures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Tools.Codegen.Generators;

public class QueryTypeGenerator : Generator
{
    /// <summary>
    /// Generates a QueryType class from a query class marked with the "query" pipeline.
    /// This class is used by the GraphQL API to execute queries.
    /// </summary>
    /// <param name="syntaxTree"></param>
    /// <param name="sourceFile"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    public override GenerationResult Generate()
    {
        const string ClassName = "QueryType__foundationCodegen";

        // If there is no target class, then return a failed generation result.
        if (Target is null)
            return new GenerationResult()
            {
                Success = false
            };

        // Get the target class marked with the generate comment.
        var @class = (ClassDeclarationSyntax)GetTarget();

        // Get all methods.
        var foundMethods = @class.ChildNodes().OfType<MethodDeclarationSyntax>();

        var replacements = new List<Tuple<SyntaxNode, SyntaxNode>>();

        // Create a new edited method with the same name and return type.
        foreach (var foundMethod in foundMethods)
        {
            var method = SyntaxFactory
                .MethodDeclaration(foundMethod.ReturnType, foundMethod.Identifier)
                .AddModifiers(foundMethod.Modifiers.ToArray())
                .AddParameterListParameters(foundMethod.ParameterList.Parameters.ToArray());

            // Adds a logger parameter to the method. This logger is to be used internally by generated code.
            // TODO: Create a Source or Incremental Generator that adds logging separately.
            var loggerType = SyntaxFactory.ParseTypeName($"ILogger<{@class.Identifier}>");
            var serviceAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Service"));

            var loggerParameter = SyntaxFactory
                .Parameter(SyntaxFactory.Identifier("__foundationCodegen_logger"))
                .WithType(loggerType)
                .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(serviceAttribute));

            method = method.AddParameterListParameters(loggerParameter);

            // Add initial execution time and database call variables.
            var initialStatements = new[]
            {
                SyntaxFactory.ParseStatement("float __foundationCodegen_time = 0;"),
                SyntaxFactory.ParseStatement("int __foundationCodegen_dbCalls = 0;"),
                SyntaxFactory.ParseStatement("var __foundationCodegen_sw = new System.Diagnostics.Stopwatch();"),
                SyntaxFactory.ParseStatement("__foundationCodegen_sw.Start();")
            };

            method = method.AddBodyStatements(initialStatements);

            // Add actual logic.
            var lastNodeIndex = foundMethod.Body!.Statements.Last() is ReturnStatementSyntax returnStatement
                ? foundMethod.Body.Statements.Count - 1
                : foundMethod.Body.Statements.Count;

            var statements = foundMethod.Body.Statements;

            // Finish by appending stopwatch stop and debug output before return or end of block.
            // TODO: Finish the concept by adding a service that receives the data.
            var finishingStatements = new[]
            {
                SyntaxFactory.ParseStatement("__foundationCodegen_sw.Stop();"),
                SyntaxFactory.ParseStatement("__foundationCodegen_time = __foundationCodegen_sw.ElapsedMilliseconds;"),
                SyntaxFactory.ParseStatement("__foundationCodegen_logger.LogDebug($\"{__foundationCodegen_time}ms, {__foundationCodegen_dbCalls} db calls\");"),
            };

            // Note: For some reason this has to be in reverse order. Oh well.
            foreach (var statement in finishingStatements.Reverse())
                statements = statements.Insert(lastNodeIndex, statement);

            method = method.AddBodyStatements(statements.ToArray());

            replacements.Add(new Tuple<SyntaxNode, SyntaxNode>(foundMethod, method));
        }

        // Replace all methods with the new edited methods.
        @class = @class.ReplaceNodes(
            replacements.Select(x => x.Item1),
            (original, _) =>
            {
                var replacement = replacements.FirstOrDefault(x => x.Item1 == original);
                return replacement?.Item2 ?? original;
            }
        );

        @class = @class.WithIdentifier(SyntaxFactory.Identifier(ClassName));

        SyntaxTree = SyntaxTree.GetCompilationUnitRoot().ReplaceNode(GetTarget(), @class).SyntaxTree;

        return new GenerationResult()
        {
            Success = true,
            Node = @class,
            ExpectedFilename = ClassName,
        };
    }

    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        return;
    }
}