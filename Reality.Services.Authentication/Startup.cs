using Microsoft.AspNetCore.Identity;
using Reality.Common.Configurations;
using Reality.Common.Data;
using Reality.Common.Entities;
using Reality.Services.Authentication.Mutations;
using Reality.Services.Authentication.Queries;
using Reality.Services.Authentication.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Reality.Services.Authentication
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

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

            // Services
            services
                .AddSingleton<IAuthenticationService, AuthenticationService>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();

            services
                .AddSingleton<JwtSecurityTokenHandler>();

            // GraphQL
            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddMutationConventions()
                .AddMongoDbFiltering()
                .AddMongoDbPagingProviders()
                .AddMongoDbProjections()
                .AddMongoDbSorting()
                .AddDefaultTransactionScopeHandler();
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
                endpoints.MapGraphQL("/api/auth");
            });
        }
    }
}