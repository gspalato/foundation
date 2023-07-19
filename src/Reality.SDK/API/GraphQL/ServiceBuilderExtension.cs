using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Reality.SDK.API.GraphQL
{
    public static class ServiceBuilderExtension
    {
        public static ServiceBuilder UseGraphQL(this ServiceBuilder builder, string path, Action<IRequestExecutorBuilder, IServiceCollection, ServiceBuilder>? configure = null)
        {
            builder.Configure((WebApplicationBuilder b) =>
            {
                var server = b.Services.AddGraphQLServer();
                server.AddMutationConventions();

                if (configure != null)
                    configure(server, b.Services, builder);
            });

            builder.Configure((WebApplication app) =>
            {
                app.MapGraphQL(path);
            });

            return builder;
        }
    }
}