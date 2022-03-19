using Microsoft.AspNetCore.Identity;
using Reality.Common.Configurations;
using Reality.Common.Data;
using Reality.Services.Identity.Mutations;
using Reality.Services.Identity.Queries;
using Reality.Services.Identity.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

namespace Reality.Services.Identity
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
                endpoints.MapGraphQL("/");
            });
        }
    }
}