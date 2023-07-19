using HotChocolate.Stitching;
using Microsoft.AspNetCore;
using Reality.SDK;
using Reality.SDK.API.GraphQL;
using Reality.SDK.Database.Mongo;
using Reality.Services.Gateway.Configurations;

namespace Reality.Services.Gateway
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new ServiceBuilder(args)
                .LoadConfiguration<GatewayConfiguration>()
                .UseMongo()
                .UseGraphQL("/gql", (server, services) =>
                {
                    var provider = services.BuildServiceProvider();
                    var logger = provider.GetRequiredService<ILogger<ServiceBuilder>>();
                    var config = provider.GetRequiredService<GatewayConfiguration>();

                    // GraphQL HTTP Schema Stitching
                    foreach (var url in config.Service_Urls.Split(","))
                    {
                        var id = url.Replace("http://", "").Replace("-", "_");
                        services.AddHttpClient(id, (_, client) =>
                        {
                            client.BaseAddress = new Uri(new Uri(url), "/gql");
                        });
                        services.AddWebSocketClient(id, (_, client) =>
                        {
                            client.Uri = new Uri(new Uri(url), "/gql");
                        });

                        server.AddRemoteSchema(id, capabilities: new EndpointCapabilities
                        {
                            Batching = BatchingSupport.RequestBatching,
                            Subscriptions = SubscriptionSupport.WebSocket
                        });

                        logger.LogDebug($"Added remote schema \"{id}\"");
                    }
                })
                .Configure((WebApplication app) =>
                {
                    app.UseCors(builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
                })
                .Build()
                .Run();
        }
    }
}
/*
namespace Reality.Services.Gateway
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
        }
    }
}
*/