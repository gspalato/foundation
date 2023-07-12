using HotChocolate.AspNetCore;
using HotChocolate.Stitching;
using NLog.Extensions.Logging;
using Reality.Services.Gateway.Configurations;
using Reality.Services.Gateway.Middleware;
using System.Reflection;

namespace Reality.Services.Gateway
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

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
            // Logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog(new Reality.Common.Configurations.LoggingConfiguration());
            });

            // Configurations
            var config = new GatewayConfiguration();
            Configuration.Bind(config);

            services.AddSingleton(config);

            // GraphQL HTTP Schema Stitching
            List<string> registeredHttpClients = new();
            foreach (var url in config.Service_Urls.Split(","))
            {
                var id = url.Replace("http://", "");
                services.AddHttpClient(id, (_, client) =>
                {
                    client.BaseAddress = new Uri(new Uri(url), "/gql");
                });
                services.AddWebSocketClient(id, (_, client) =>
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
                graphQlServer.AddRemoteSchema(id.Replace('-', '_'), capabilities: new EndpointCapabilities
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

            app.UseCors(options =>
            {
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
