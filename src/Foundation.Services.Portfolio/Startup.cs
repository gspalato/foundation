/*
using HotChocolate.AspNetCore;
using NLog.Extensions.Logging;
using Octokit;
using Octokit.Internal;
using Foundation.Common.Configurations;
using Foundation.Common.Data;
using Foundation.Services.Portfolio.Configurations;
using Foundation.Services.Portfolio.Repositories;
using Foundation.Services.Portfolio.Services;
using Project = Foundation.Common.Entities.Project;

namespace Foundation.Services.Portfolio
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
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
                loggingBuilder.AddNLog(new LoggingConfiguration());
            });

            // Configurations
            var config = new Configuration();
            Configuration.Bind(config);

            services.AddSingleton(config);

            // Database Repositories
            var databaseContext = new DatabaseContext(config);
            services.AddSingleton<IDatabaseContext, DatabaseContext>(_ => databaseContext);

            services.AddSingleton<IProjectRepository, ProjectRepository>();

            // Services
            var productInfo = new ProductHeaderValue("FoundationAPI", "1.0.0");
            var credentials = new Credentials(config.GithubToken, AuthenticationType.Bearer);
            var store = new InMemoryCredentialStore(credentials);

            services.AddSingleton<GitHubClient>(_ => new GitHubClient(productInfo, store));

            services.AddHostedService<ProjectService>();

            // GraphQL
            services
                .AddGraphQLServer()
                .AddQueryType<Types.Query>()
                .AddType<Project>()
                .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);
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