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
        const string MethodName = "GetIntrospectionInfoAsync";
        const string GraphQLName = "_foundation_introspectionInfo";

        if (Target is null)
            return new GenerationResult()
            {
                Success = false
            };

        var @class = (ClassDeclarationSyntax)GetTarget();

        // Check if class already has a method called "GetIntrospectionInfoAsync".
        var hasIntrospectionMethod = @class.ChildNodes().OfType<MethodDeclarationSyntax>().Any(m =>
        {
            // Check if method is called "GetIntrospectionInfoAsync" or "__foundation_introspectionInfo".
            if (m.Identifier.ValueText == MethodName
                || m.Identifier.ValueText == GraphQLName)
                return true;
            
            // Check if method has a GraphQLName attribute with the value "__foundation_introspectionInfo".
            if (m.AttributeLists.Any(
                a => a.Attributes.Any(a =>
                    a.Name.ToString() == "HotChocolate.GraphQLName"
                    && a.ArgumentList is not null
                    && a.ArgumentList.Arguments.Any(a => a.Expression.ToFullString() == GraphQLName)
                )
            ))
                return true;

            return false;
        });

        if (hasIntrospectionMethod)
            return new GenerationResult()
            {
                Success = true
            };

        // Add custom GraphQL schema name instead of the method name using the HotChocolate.GraphQLName attribute.
        var nameAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("HotChocolate.GraphQLName"));
        var nameArgument = SyntaxFactory.AttributeArgument(
            SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(GraphQLName)
            )
        );

        nameAttribute = nameAttribute.AddArgumentListArguments(nameArgument);

        var methodAttributeList = SyntaxFactory.AttributeList().AddAttributes(nameAttribute);

        // Create new method called "GetIntrospectionInfoAsync" using SyntaxFactory.
        StatementSyntax[] statements = new[]
        {
            SyntaxFactory.ParseStatement("return __foundationCodegen_serviceInfo;")
        };

        var serviceInfoType = SyntaxFactory.ParseTypeName($"Foundation.Core.SDK.ServiceInfo");
        var serviceAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Service"));

        var serviceInfoParameter = SyntaxFactory
            .Parameter(SyntaxFactory.Identifier("__foundationCodegen_serviceInfo"))
            .WithType(serviceInfoType)
            .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(serviceAttribute));

        var method = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName("Foundation.Core.SDK.ServiceInfo"), MethodName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(serviceInfoParameter)
            .AddBodyStatements(statements)
            .AddAttributeLists(methodAttributeList);

        @class = @class.AddMembers(method);

        SyntaxTree = SyntaxTree.GetCompilationUnitRoot().ReplaceNode(GetTarget(), @class).SyntaxTree;

        return new GenerationResult()
        {
            Success = true,
            Node = @class,
            ExpectedFilename = Path.GetFileNameWithoutExtension(SourceFile.Name),
        };
    }
}