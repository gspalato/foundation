using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Foundation.Core.Codegen.API.GraphQL;

/// <summary>
/// Stitches together all classes with the [Query] attribute and generates a new class called QueryType in the same namespace.
/// The methods/queries are modified to include a execution time and database call tracking.
/// More functionality can be added by modifying this source generator.
/// </summary>
[Generator]
public class QueryTypeSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new QueryTypeSyntaxReceiver());

        if (!Debugger.IsAttached)
            Debugger.Launch();
    }

    public void Execute(GeneratorExecutionContext context)
    {
        Console.WriteLine("Executing QueryTypeSourceGenerator.");

        if (context.SyntaxReceiver is not QueryTypeSyntaxReceiver receiver)
            return;

        // Get the compilation that the generator is being executed in.
        var compilation = context.Compilation;

        var queryMethods = new List<MethodDeclarationSyntax>();

        var usingDirectives = new List<UsingDirectiveSyntax>();

        foreach (var foundQueryClass in receiver.CandidateClasses)
        {
            // Get all the using statements.
            var foundUsings = foundQueryClass.SyntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>();
            usingDirectives.AddRange(foundUsings);

            // Get all methods.
            var foundMethods = foundQueryClass.ChildNodes().OfType<MethodDeclarationSyntax>();

            // Create a new edited method with the same name and return type.
            foreach (var foundMethod in foundMethods)
            {
                var method = SyntaxFactory
                    .MethodDeclaration(foundMethod.ReturnType, foundMethod.Identifier)
                    .AddModifiers(foundMethod.Modifiers.ToArray())
                    .AddParameterListParameters(foundMethod.ParameterList.Parameters.ToArray());

                var initialStatements = new[]
                {
                    SyntaxFactory.ParseStatement("float __foundationCodegen_time = 0;"),
                    SyntaxFactory.ParseStatement("int __foundationCodegen_dbCalls = 0;"),
                    SyntaxFactory.ParseStatement("var __foundationCodegen_sw = new System.Diagnostics.Stopwatch();"),
                    SyntaxFactory.ParseStatement("__foundationCodegen_sw.Start();")
                };

                method = method.AddBodyStatements(initialStatements);

                // Add actual logic.
                var lastNodeIndex = foundMethod.Body.Statements.Last() is ReturnStatementSyntax returnStatement
                    ? foundMethod.Body.Statements.Count() - 1
                    : foundMethod.Body.Statements.Count();

                var statements = foundMethod.Body.Statements;

                // Finish by appending stopwatch stop and debug output before return or end of block.
                // Note: For some reason this has to be in inverted order. Oh well.
                // TODO: Finish the concept by adding a service that receives the data.
                var finishingStatements = new[]
                {
                    SyntaxFactory.ParseStatement("System.Diagnostics.Debug.WriteLine($\"{__foundationCodegen_time}ms, {__foundationCodegen_dbCalls} db calls\");"),
                    SyntaxFactory.ParseStatement("__foundationCodegen_sw.Stop();"),
                    SyntaxFactory.ParseStatement("__foundationCodegen_time = __foundationCodegen_sw.ElapsedMilliseconds;"),
                };

                foreach (var statement in finishingStatements)
                    statements = statements.Insert(lastNodeIndex, statement);

                method = method.AddBodyStatements(statements.ToArray());
                queryMethods.Add(method);
            }
        }

        // Find namespace node so it can be used to generate the source code in the same namespace.
        // If the class is not in a namespace, then use the first namespace in the compilation.
        // These are added to a namespaces list, so that the most common namespace can be used.
        // In theory this should be the same for all classes, but it's possible that it's not.
        // If no namespace is found, then the class is declared in the global namespace.
        string namespaceIdentifier = "";
        if (receiver.CandidateNamespaces.Count != 0)
        {
            var commonNamespace = receiver.CandidateNamespaces
                .Where(n => n is not null)
                .GroupBy(n =>
                {
                    if (n is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                        return namespaceDeclarationSyntax.Name.ToFullString();

                    if (n is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax)
                        return baseNamespaceDeclarationSyntax.Name.ToFullString();

                    return "";
                })
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()   // Get the group with the most items.
                .FirstOrDefault();  // Get the first item in the group, as they should all be the same.

            if (commonNamespace is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                namespaceIdentifier = namespaceDeclarationSyntax.Name.ToFullString();
            else if (commonNamespace is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax)
                namespaceIdentifier = baseNamespaceDeclarationSyntax.Name.ToFullString();
        }
        else
            System.Diagnostics.Debug.WriteLine("No namespace found any class. Using global namespace.");


        // Generate source code.
        // Note: This was the best working solution yet.
        //       Directly using the SyntaxFactory to create the source code was not working.
        //       It didn't add spaces between keywords properly.
        var sourceBuilder = new StringBuilder();

        sourceBuilder.AppendLine("// This file was automatically generated by Foundation.");
        sourceBuilder.AppendLine("// It is a compilation of all [Query] classes, related to GraphQL APIs.");
        sourceBuilder.AppendLine("// All methods were modified to include a execution time and database call tracking.");
        sourceBuilder.AppendLine("// [Do not modify this file directly, as it will be overwritten.]");
        sourceBuilder.AppendLine("// [Do not check this file into source control.]");

        usingDirectives.ForEach(d => sourceBuilder.Append(d.ToFullString()));

        sourceBuilder.AppendLine($"namespace {namespaceIdentifier};");

        sourceBuilder.AppendLine("public class QueryType\n{");

        foreach (var method in queryMethods)
            sourceBuilder.AppendLine(method.ToFullString());

        sourceBuilder.AppendLine("}");

        // Add the source code to the compilation
        var source = SourceText.From(FormatCode(sourceBuilder.ToString()), Encoding.UTF8);
        context.AddSource($"QueryType.g.cs", source);
    }

    /// <summary>
    /// Formats the code to be more human readable.
    /// </summary>
    /// <param name="code">The code to format.</param>
    /// <param name="cancelToken">The cancellation token.</param>
    /// <returns>The formatted code.</returns>
    /// <remarks>
    /// Even though is code is autogenerated and will only be ran by the compiler,
    /// it's still nice to have it formatted properly. This is especially useful for debugging.
    /// </remarks>
    private static string FormatCode(string code, CancellationToken cancelToken = default)
    {
        return CSharpSyntaxTree.ParseText(code, cancellationToken: cancelToken)
            .GetRoot(cancelToken)
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText(cancelToken)
            .ToString();
    }
}

public class QueryTypeSyntaxReceiver : ISyntaxReceiver
{
    public List<SyntaxNode> CandidateNamespaces { get; } = new();
    public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
            && IsQueryClass(classDeclarationSyntax))
        {
            CandidateClasses.Add(classDeclarationSyntax);
        }

        if (syntaxNode is NamespaceDeclarationSyntax namespaceDeclarationSyntax && ContainsQueryClass(syntaxNode))
            CandidateNamespaces.Add(namespaceDeclarationSyntax);

        if (syntaxNode is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax && ContainsQueryClass(syntaxNode.Parent))
            CandidateNamespaces.Add(baseNamespaceDeclarationSyntax);
    }

    private static bool IsQueryClass(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return classDeclarationSyntax.AttributeLists.SelectMany(l => l.Attributes).Any(a => a.Name.ToString() is "Query");
    }

    private static bool ContainsQueryClass(SyntaxNode node)
    {
        return node
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Any(c => IsQueryClass(c));
    }
}