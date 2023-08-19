using HotChocolate.Stitching;
using Microsoft.AspNetCore.Cors.Infrastructure;
using NLog;
using NLog.Config;
using Foundation.Core.SDK;
using Foundation.Core.SDK.API.GraphQL;
using Foundation.Core.SDK.Database.Mongo;

new ServiceBuilder(args)
    .UseMongo()
    .UseGraphQL("/gql", (server, services, builder) =>
    {
        Logger logger = LogManager
            .Setup()
            .LoadConfiguration(new LoggingConfiguration())
            .GetCurrentClassLogger();

        server.AddErrorFilter(error =>
        {
            logger.Error(error.Exception, error.Exception!.Message);
            return error;
        });

        // GraphQL HTTP Schema Stitching
        // TODO: Implement proper service discovery.
        var urls = builder.Configuration.GetValue<string>("ServiceUrls");
        if (urls is null || urls == string.Empty)
            throw new Exception("ServiceUrls is not defined.");

        foreach (var url in urls.Split(","))
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

            logger.Info($"GraphQL Schema Stitched: {id} -> {url}");
        }
    })
    .UseGraphQLPlayground("/ui")
    .AddCors("AllowAll", new CorsPolicyBuilder()
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .Build())
    .Build()
    .Run();