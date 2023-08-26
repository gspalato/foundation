using Microsoft.AspNetCore.Cors.Infrastructure;
using NLog;
using NLog.Config;
using Foundation.Core.SDK;
using Foundation.Core.SDK.API.GraphQL;
using Foundation.Core.SDK.Database.Mongo;
using HotChocolate.Language;

new ServiceBuilder(args)
    .WithName("Gateway")
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

            // Stitch schema but ignoring the service info field.
            server
                .AddRemoteSchema(id)
                .IgnoreField("Query", "serviceInfo", id)
                .IgnoreField("Query", $"{id}_serviceInfo", id);

            Console.WriteLine($"Stitched schema {id} from {url}.");
            Console.WriteLine($"Ignoring field serviceInfo and {id}_serviceInfo.");

            logger.Info($"GraphQL Schema Stitched: {id} -> {url}");
        }

        server
            .IgnoreField("Query", "serviceInfo");
    })
    .UseGraphQLPlayground("/ui")
    .AddCors(
        "AllowAll",
        new CorsPolicyBuilder()
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .Build()
    )
    .Build()
    .Run();