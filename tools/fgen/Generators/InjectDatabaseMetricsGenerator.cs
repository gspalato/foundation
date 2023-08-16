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

public class InjectDatabaseMetricsGenerator : Generator
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

        var @class = Target;

        var replacements = new List<Tuple<SyntaxNode, SyntaxNode>>();

        // Add to the database call counter after every database call.
        int CheckMethodForDatabaseCalls(MethodDeclarationSyntax m)
        {
            var invocations = new List<InvocationExpressionSyntax>();

            foreach (var node in m.DescendantNodes())
            {
                if (node is LocalDeclarationStatementSyntax declarationSyntax)
                    invocations.AddRange(declarationSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>());
                else if (node is InvocationExpressionSyntax invocationSyntax)
                    invocations.Add(invocationSyntax);
            }

            var dbCalls = 0;
            foreach (var invocation in invocations)
            {
                var symbol = SemanticModel().GetDeclaredSymbol(invocation);
                Console.WriteLine($"Symbol for \"{invocation.ToFullString()}\" :: {symbol?.Name}");

                if (symbol is not IMethodSymbol methodSymbol)
                    continue;

                Console.WriteLine("Found invocation: " + methodSymbol.Name);

                if (methodSymbol.ContainingType.BaseType?.Name == "Repository"
                && methodSymbol.ContainingType.ContainingNamespace.Name.StartsWith("Foundation.Core.SDK.Database"))
                    dbCalls++;

                if (methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not MethodDeclarationSyntax invocationMethod)
                    continue;

                dbCalls += CheckMethodForDatabaseCalls(invocationMethod);
            }

            return dbCalls;
        }

        foreach (var method in @class.ChildNodes().OfType<MethodDeclarationSyntax>())
        {
            var calls = CheckMethodForDatabaseCalls(method);
            if (calls > 0)
            {
                
            }
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

        return new GenerationResult()
        {
            Success = true,
            Node = @class,
        };
    }

    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode) { }
}