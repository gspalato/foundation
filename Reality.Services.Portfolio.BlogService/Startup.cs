using HotChocolate.AspNetCore;
using Reality.Common.Configurations;
using Reality.Common.Data;
using Reality.Services.Portfolio.BlogService.Queries;
using Reality.Services.Portfolio.BlogService.Repositories;
using Reality.Services.Portfolio.BlogService.Types;
using System.Reflection;

namespace Reality.Services.Portfolio.BlogService
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
            // Configurations
            var config = new BaseConfiguration();
            Configuration.Bind(config);

            services.AddSingleton(config);

            // Database Repositories
            var databaseContext = new DatabaseContext(config);
            services.AddSingleton<IDatabaseContext, DatabaseContext>(_ => databaseContext);

            services
                .AddScoped<IBlogPostRepository, BlogPostRepository>();

            // GraphQL
            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddType<BlogPostType>()
                .AddMongoDbFiltering()
                .AddMongoDbPagingProviders()
                .AddMongoDbProjections()
                .AddMongoDbSorting();
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