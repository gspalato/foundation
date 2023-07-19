using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.SDK.API.GraphQL
{
    public static class ServiceBuilderExtension
    {
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
}