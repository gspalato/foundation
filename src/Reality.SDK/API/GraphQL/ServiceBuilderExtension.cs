using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;

namespace Reality.SDK.API.GraphQL
{
    public static class ServiceBuilderExtension
    {
        public static ServiceBuilder UseGraphQL(this ServiceBuilder builder, string path, Action<IRequestExecutorBuilder, IServiceCollection>? configure = null)
        {
            builder.Configure((WebApplicationBuilder b) =>
            {
                var server = b.Services.AddGraphQLServer();

                if (configure != null)
                    configure(server, b.Services);
            });

            builder.Configure((WebApplication app) =>
            {
                app.MapGraphQL(path);
            });

            return builder;
        }
    }
}