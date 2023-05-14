using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using HotChocolate.AspNetCore;
using Reality.Common.Configurations;
using Reality.Common.Data;
using Reality.Gateway.Configurations;
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

            // Database Repositories
            var databaseContext = new DatabaseContext(config);
            services.AddSingleton(_ => databaseContext);

            // Hangfire
            /*
            services
                .AddHangfire(configuration =>
                {
                    var client = databaseContext.GetClient();

                    configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseMongoStorage(client, "hangfire", new MongoStorageOptions
                    {
                        MigrationOptions = new MongoMigrationOptions
                        {
                            MigrationStrategy = new MigrateMongoMigrationStrategy(),
                            BackupStrategy = new CollectionMongoBackupStrategy()
                        },
                        Prefix = "hangfire.mongo.gateway",
                        CheckConnection = true,
                        CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
                    });
                })
                .AddHangfireServer(serverOptions =>
                {
                    serverOptions.ServerName = "reality.hangfire.gateway";
                });
            */

            GlobalConfiguration.Configuration.UseColouredConsoleLogProvider();

            List<string> registeredHttpClients = new();
            foreach (var url in config.Service_Urls.Split(","))
            {
                var id = url.Replace("http://", "");
                services.AddHttpClient(id, (sp, client) =>
                {
                    client.BaseAddress = new Uri(new Uri(url), "/gql");
                });
                registeredHttpClients.Add(id);
            }

            var graphQlServer = services.AddGraphQLServer();

            foreach (var id in registeredHttpClients)
            {
                graphQlServer.AddRemoteSchema(id);
                Console.WriteLine($"Added remote schema \"{id}\"");
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

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
