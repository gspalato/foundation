using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Reality.Common.Configurations;
using Reality.Common.Services;
using Reality.SDK;
using Reality.SDK.API.GraphQL;
using Reality.SDK.Database.Mongo;
using Reality.Services.Identity.Services;
using Reality.Services.Identity.Types;
using System.IdentityModel.Tokens.Jwt;

new ServiceBuilder(args)
    .BindConfiguration<BaseConfiguration>()
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