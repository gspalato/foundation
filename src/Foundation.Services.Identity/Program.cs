using Microsoft.AspNetCore.Identity;
using Foundation.Common.Configurations;
using Foundation.Core.SDK;
using Foundation.Core.SDK.API.GraphQL;
using Foundation.Core.SDK.Auth.JWT;
using Foundation.Core.SDK.Database.Mongo;
using Foundation.Services.Identity.Services;
using Foundation.Services.Identity.Types;
using System.Reflection;

new ServiceBuilder(args)
    .BindConfiguration<IBaseConfiguration, BaseConfiguration>()
    .UseMongo()
    .UseGraphQL("/gql", (server, services, builder) =>
    {
        server.AddGeneratedQueryType();
        server.AddMutationType<Mutation>();

        server
            .AddMongoDbFiltering()
            .AddMongoDbPagingProviders()
            .AddMongoDbProjections()
            .AddMongoDbSorting()
            .AddDefaultTransactionScopeHandler();
    })
    .UseJWT()
    .Configure((WebApplicationBuilder appBuilder) =>
    {
        appBuilder.Services
            .AddSingleton<IAuthenticationService, AuthenticationService>()
            .AddSingleton<IUserService, UserService>()
            .AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();
    })
    .Build()
    .Run();