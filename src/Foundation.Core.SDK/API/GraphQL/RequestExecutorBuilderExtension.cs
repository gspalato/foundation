using System.Reflection;
using Foundation.Common;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Core.SDK.API.GraphQL;

public static class RequestExecutorBuilderExtension
{
    /// <summary>
    ///   Adds the generated query type to the GraphQL server.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <remarks>
    ///  This method requires the use of the <c>fgen</c> tool to generate the query type before compiling the code.
    /// </remarks>
    public static IRequestExecutorBuilder AddGeneratedQueryType(this IRequestExecutorBuilder builder)
    {
        var queryType = Assembly
            .GetCallingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name == "__foundationCodegen_QueryType" && t.IsClass && t.GetCustomAttributes<GeneratedAttribute>().Any());
            
        if (queryType is null)
            throw new Exception("Could not find generated query type. Did you run the fgen tool?");

        builder.AddQueryType(queryType);

        return builder;
    }

    /// <summary>
    ///   Adds the generated mutation type to the GraphQL server.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <remarks>
    ///  This method requires the use of the <c>fgen</c> tool to generate the query type before compiling the code.
    /// </remarks>
    public static IRequestExecutorBuilder AddGeneratedMutationType(this IRequestExecutorBuilder builder)
    {
        var queryType = Assembly
            .GetCallingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name == "__foundationCodegen_MutationType" && t.IsClass && t.GetCustomAttributes<GeneratedAttribute>().Any());
            
        if (queryType is null)
            throw new Exception("Could not find generated query type. Did you run the fgen tool?");

        builder.AddQueryType(queryType);

        return builder;
    }
}