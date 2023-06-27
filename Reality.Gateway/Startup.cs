using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using HotChocolate.AspNetCore;
using HotChocolate.Stitching;
using Reality.Common.Configurations;
using Reality.Common.Data;
using Reality.Gateway.Configurations;
using Reality.Gateway.Middleware;
using System.Reflection;

namespace Reality.Gateway
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var config = new GatewayConfiguration();
            Configuration.Bind(config);

            services.AddSingleton(config);

            GlobalConfiguration.Configuration.UseColouredConsoleLogProvider();

            // GraphQL HTTP Schema Stitching
            List<string> registeredHttpClients = new();
            foreach (var url in config.Service_Urls.Split(","))
            {
                var id = url.Replace("http://", "");
                services.AddHttpClient(id, (sp, client) =>
                {
                    client.BaseAddress = new Uri(new Uri(url), "/gql");
                });
                services.AddWebSocketClient(id, (sp, client) =>
                {
                    client.Uri = new Uri(new Uri(url), "/gql");
                });
                registeredHttpClients.Add(id);
            }

            var graphQlServer = services
                .AddGraphQLServer()
                .AddDiagnosticEventListener<ErrorLoggingDiagnosticsEventListener>();

            foreach (var id in registeredHttpClients)
            {
                graphQlServer.AddRemoteSchema(id, capabilities: new EndpointCapabilities
                {
                    Batching = BatchingSupport.RequestBatching,
                    Subscriptions = SubscriptionSupport.WebSocket
                });
                Console.WriteLine($"Added remote schema \"{id}\"");
            }

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(options => {
                options
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapGraphQL("/gql")
                    .WithOptions(new GraphQLServerOptions
                    {
                        Tool = { Enable = false }
                    });
                
                endpoints
                    .MapBananaCakePop("/ui");
            });
        }
    }
}
