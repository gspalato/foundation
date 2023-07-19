using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using NLog.Extensions.Logging;
using Foundation.Common.Configurations;
using Foundation.Common.Data;
using Foundation.Common.Services;
using Foundation.Services.Identity.Services;
using Foundation.Services.Identity.Types;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

namespace Foundation.Services.Identity
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
            // Logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                loggingBuilder.AddNLog(new Foundation.Common.Configurations.LoggingConfiguration());
            });

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
                .AddSingleton<IAuthorizationService, AuthorizationService>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();

            services
                .AddSingleton<JwtSecurityTokenHandler>()
                .AddAuthorization()
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => options.TokenValidationParameters = Foundation.Common.Configurations.TokenConfiguration.ValidationParameters);

            // GraphQL
            services
                .AddGraphQLServer()
                .AddAuthorization()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddMutationConventions()
                .AddMongoDbFiltering()
                .AddMongoDbPagingProviders()
                .AddMongoDbProjections()
                .AddMongoDbSorting()
                .AddDefaultTransactionScopeHandler()
                .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
                .AddResolver("Query", "user", (context) =>
                {
                    return context.GetUser();
                })
                .AddResolver("Mutation", "user", (context) =>
                {
                    return context.GetUser();
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

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