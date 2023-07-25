using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Core.SDK.API.GraphQL;

public static class ServiceBuilderExtension
{
    public static ServiceBuilder AddQuery<T>(this ServiceBuilder builder) where T : class
    {
        builder.Configure((WebApplicationBuilder b) =>
        {
            using (var provider = b.Services.BuildServiceProvider())
            {
                var gql = provider.GetRequiredService<IRequestExecutorBuilder>();

                // Generate a proxy type for the query type that tracks the method's execution time
                // as well as database calls.

                gql.AddQueryType<T>();
            }
        });

        return builder;
    }

    public static ServiceBuilder AddMutation<T>(this ServiceBuilder builder) where T : class
    {
        builder.Configure((WebApplicationBuilder b) =>
        {
            using (var provider = b.Services.BuildServiceProvider())
            {
                var gql = provider.GetRequiredService<IRequestExecutorBuilder>();

                // Generate a proxy type for the mutation type that tracks the method's execution time
                // as well as database calls.

                gql.AddMutationType<T>();
            }
        });

        return builder;
    }

    public static ServiceBuilder AddSubscription<T>(this ServiceBuilder builder) where T : class
    {
        builder.Configure((WebApplicationBuilder b) =>
        {
            using (var provider = b.Services.BuildServiceProvider())
            {
                var gql = provider.GetRequiredService<IRequestExecutorBuilder>();

                // Generate a proxy type for the subscription type that tracks the method's execution time
                // as well as database calls.

                gql.AddSubscriptionType<T>();
            }
        });

        return builder;
    }

    public static ServiceBuilder UseGraphQL(this ServiceBuilder builder, string path, Action<IRequestExecutorBuilder, IServiceCollection, ServiceBuilder>? configure = null)
    {
        builder.Configure((WebApplicationBuilder b) =>
        {
            var server = b.Services.AddGraphQLServer();
            server.AddDiagnosticEventListener<DiagnosticEventListener>();

            b.Services.AddErrorFilter<ErrorFilter>();

            if (configure != null)
                configure(server, b.Services, builder);
        });

        builder.Configure((WebApplication app) =>
        {
            app.MapGraphQL(path).WithOptions(new GraphQLServerOptions
            {
                Tool = { Enable = false }
            });
        });

        return builder;
    }

    public static ServiceBuilder UseGraphQLPlayground(this ServiceBuilder builder, string path)
    {
        builder.Configure((WebApplication app) =>
        {
            app.MapBananaCakePop(path).WithOptions(new GraphQLToolOptions
            {
                Enable = true,
                GraphQLEndpoint = path
            });
        });

        return builder;
    }
}
