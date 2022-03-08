using Reality.Common.Configurations;
using Reality.Common.Data;
using Reality.Services.Database.Queries;
using Reality.Services.Database.Repositories;
using Reality.Services.Database.Types;
using System.Reflection;

namespace Reality.Services.Database
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddUserSecrets(Assembly.GetExecutingAssembly());

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configurations
            var mongoConfig = new MongoDbConfiguration();
            Configuration.GetSection(nameof(MongoDbConfiguration)).Bind(mongoConfig);

            services.AddSingleton(mongoConfig);

            // Database Repositories
            var databaseContext = new DatabaseContext(mongoConfig);
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
                endpoints.MapGraphQL("/");
            });
        }
    }
}