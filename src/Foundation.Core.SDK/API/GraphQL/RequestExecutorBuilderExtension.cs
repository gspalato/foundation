using System.Reflection;
using Foundation.Common;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Core.SDK.API.GraphQL;

public static class RequestExecutorBuilderExtension
{
    public static IRequestExecutorBuilder AddGeneratedQueryType(this IRequestExecutorBuilder builder)
    {
        var queryType = Assembly
            .GetCallingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name == "__foundationCodegen_QueryType" && t.IsClass && t.GetCustomAttributes<GeneratedAttribute>().Any());
            
        if (queryType is null) throw new Exception("Could not find generated query type.");

        builder.AddQueryType(queryType);

        return builder;
    }

    public static IRequestExecutorBuilder AddGeneratedMutationType(this IRequestExecutorBuilder builder)
    {
        var queryType = Assembly
            .GetCallingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name == "__foundationCodegen_MutationType" && t.IsClass && t.GetCustomAttributes<GeneratedAttribute>().Any());
            
        if (queryType is null) throw new Exception("Could not find generated query type.");

        builder.AddQueryType(queryType);

        return builder;
    }
}