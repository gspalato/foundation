using HotChocolate.AspNetCore;
using HotChocolate.Subscriptions;
using NLog.Extensions.Logging;
using Reality.Common.Configurations;
using Reality.Common.Data;
using Reality.Common.Entities;
using Reality.Common.Services;
using Reality.Services.UPx.Types;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

/*
namespace Reality.Services.UPx
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

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
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                loggingBuilder.AddNLog(new Reality.Common.Configurations.LoggingConfiguration());
            });

            // Configurations
            var config = new BaseConfiguration();
            Configuration.Bind(config);

            services.AddSingleton(config);

            // Database Repositories
            var databaseContext = new DatabaseContext(config);
            services.AddSingleton<IDatabaseContext, DatabaseContext>(_ => databaseContext);

            services
                .AddScoped<IUseRepository, UseRepository>();

            // GraphQL
            services
                .AddGraphQLServer()
                .AddInMemorySubscriptions()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddSubscriptionType<Subscription>()
                .AddType<Use>()
                .AddType<Resume>()
                .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
                .AddMongoDbFiltering()
                .AddMongoDbPagingProviders()
                .AddMongoDbProjections()
                .AddMongoDbSorting();

            services
                .AddSingleton<IAuthorizationService, AuthorizationService>();

            services
                .AddSingleton<JwtSecurityTokenHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseWebSockets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL("/gql")
                    .WithOptions(new GraphQLServerOptions()
                    {
                        Tool = { Enable = false }
                    });
            });
        }
    }
}
*/