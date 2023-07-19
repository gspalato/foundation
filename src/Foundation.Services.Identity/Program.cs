using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Foundation.Common.Configurations;
using Foundation.Common.Services;
using Foundation.SDK;
using Foundation.SDK.API.GraphQL;
using Foundation.SDK.Database.Mongo;
using Foundation.Services.Identity.Services;
using Foundation.Services.Identity.Types;
using System.IdentityModel.Tokens.Jwt;

new ServiceBuilder(args)
    .BindConfiguration<IBaseConfiguration, BaseConfiguration>()
    .UseMongo()
    .UseGraphQL("/gql", (server, services, builder) =>
    {
        server.AddQueryType<Query>();
        server.AddMutationType<Mutation>();

        server
            .AddMongoDbFiltering()
            .AddMongoDbPagingProviders()
            .AddMongoDbProjections()
            .AddMongoDbSorting()
            .AddDefaultTransactionScopeHandler();
    })
    .Configure((WebApplicationBuilder appBuilder) =>
    {
        appBuilder.Services
            .AddSingleton<IAuthenticationService, AuthenticationService>()
            .AddSingleton<IAuthorizationService, AuthorizationService>()
            .AddSingleton<IUserService, UserService>()
            .AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();

        appBuilder.Services
            .AddSingleton<JwtSecurityTokenHandler>()
            .AddAuthorization()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = TokenConfiguration.ValidationParameters);
    })
    .Build()
    .Run();