using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Reality.Common.Configurations;
using Reality.Services.Identity.Repositories;
using Reality.Services.Identity.Services;
using Reality.Services.Identity.Types;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

using RCommonServices = Reality.Common.Services;

namespace Reality.Services.Identity
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
            // Configurations
            var config = new BaseConfiguration();
            Configuration.Bind(config);

            services.AddSingleton(config);

            var credentials = new BasicAWSCredentials(config.AwsAccessKeyId, config.AwsSecretAccessKey);
            var client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.USEast1);

            services
                .AddSingleton<AWSCredentials, BasicAWSCredentials>(_ => credentials)
                .AddSingleton<IAmazonDynamoDB>(_ => client)
                .AddSingleton<IDynamoDBContext, DynamoDBContext>();

            services
                .AddSingleton<UserRepository>();

            // Services
            services
                .AddSingleton<IAuthenticationService, AuthenticationService>()
                .AddSingleton<RCommonServices::IAuthorizationService, RCommonServices::AuthorizationService>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();

            services
                .AddSingleton<JwtSecurityTokenHandler>()
                .AddAuthorization()
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => options.TokenValidationParameters = Reality.Common.Configurations.TokenConfiguration.ValidationParameters);

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