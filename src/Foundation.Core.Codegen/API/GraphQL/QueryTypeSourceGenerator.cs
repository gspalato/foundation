using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Foundation.Core.Codegen.API.GraphQL;

[Generator]
public class QueryTypeSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        Console.WriteLine("Executing QueryTypeSourceGenerator");

        // Find the query class.
        var foundTrees = context.Compilation.SyntaxTrees.Where(tree =>
            tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Any(p =>
                    p
                        .DescendantNodes()
                        .OfType<AttributeSyntax>()
                        .Any()
                )
        );

        var foundClasses = foundTrees.SelectMany(tree =>
            tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
        ).SelectMany(@class =>
            @class
                .DescendantNodes()
                .OfType<AttributeSyntax>()
        )
        .Where(attr =>
            attr
                .Name
                .GetText()
                .ToString() == "GenerateQuery"
        ).Select(attr => attr.Parent).OfType<ClassDeclarationSyntax>();

        var foundQueryClass = foundClasses.FirstOrDefault();
        Console.WriteLine("Found query class: " + foundQueryClass.Identifier);

        // Find namespace node so it can be used to generate the source code in the same namespace.
        // If the class is not in a namespace, then use the first namespace in the compilation.
        SyntaxNode foundNamespaceNode = foundQueryClass.Parent;
        if (foundNamespaceNode.GetFirstToken().Text != "namespace")
            foundNamespaceNode = foundQueryClass.SyntaxTree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

        // Get all the using statements.
        var foundUsings = foundQueryClass.SyntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>();
        var foundUsingsSource = foundUsings.Select(u => u.GetText()).ToList();

        // Get all methods.
        var foundMethods = foundQueryClass.ChildNodes().OfType<MethodDeclarationSyntax>();

        // Create new class and copy the methods, adding the code to track the execution time and database calls.
        // Note: since C# source code is immutable, these changes return new syntaxes.
        var queryClass = SyntaxFactory.ClassDeclaration("QueryType")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(foundMethods.ToArray());

        foreach (var foundMethod in foundMethods)
        {
            var method = SyntaxFactory.MethodDeclaration(foundMethod.ReturnType, foundMethod.Identifier)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(foundMethod.ParameterList.Parameters.ToArray())
                .AddBodyStatements(foundMethod.Body.Statements.ToArray());

            method = method.InsertNodesBefore(foundMethod.Body.Statements.First(), new[]
            {
                    SyntaxFactory.ParseStatement("float __foundationCodegen_time = 0;"),
                    SyntaxFactory.ParseStatement("int __foundationCodegen_dbCalls = 0;"),
                    SyntaxFactory.ParseStatement("var __foundationCodegen_sw = new System.Diagnostics.Stopwatch();"),
                    SyntaxFactory.ParseStatement("__foundationCodegen_sw.Start();")
            });

            var lastNode = foundMethod.Body.Statements.Last().GetFirstToken().Text is "return"
                ? foundMethod.Body.Statements[foundMethod.Body.Statements.Count() - 2]
                : foundMethod.Body.Statements.Last();

            method = method.InsertNodesAfter(lastNode, new[]
            {
                    SyntaxFactory.ParseStatement("__foundationCodegen_sw.Stop();"),
                    SyntaxFactory.ParseStatement("__foundationCodegen_time = __foundationCodegen_sw.ElapsedMilliseconds;"),
                    SyntaxFactory.ParseStatement("System.Diagnostics.Debug.WriteLine($\"{__foundationCodegen_time}ms, {__foundationCodegen_dbCalls} db calls\");")
            });

            queryClass = queryClass.AddMembers(method);
        }

        // Generate the source code
        var namespaceNode = foundNamespaceNode.InsertTokensAfter(foundNamespaceNode.GetLastToken(), new SyntaxToken[]
        {
                SyntaxFactory.ParseToken("."),
                SyntaxFactory.ParseToken("Codegen")
        });
        var namespaceSource = namespaceNode.GetText().ToString();

        var classSource = queryClass.GetText().ToString();

        var sourceBuilder = new StringBuilder();
        foreach (var usingSource in foundUsingsSource)
        {
            sourceBuilder.AppendLine(usingSource.ToString());
        }

        sourceBuilder.AppendLine(namespaceSource.ToString());

        sourceBuilder.AppendLine(classSource.ToString());

        // Add the source code to the compilation
        context.AddSource($"QueryType.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));

        // Test.
        context.AddSource($"Test.g.cs", SourceText.From(@"Hello world!", Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        if (!Debugger.IsAttached)
            Debugger.Launch();
    }
}
