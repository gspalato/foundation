using System.IdentityModel.Tokens.Jwt;
using Foundation.Common.Configurations;
using Foundation.Common.Entities;
using Foundation.Common.Services;
using Foundation.Core.SDK;
using Foundation.Core.SDK.API.GraphQL;
using Foundation.Core.SDK.Database.Mongo;
using Foundation.Services.UPx.Types;

new ServiceBuilder(args)
    .BindConfiguration<IBaseConfiguration, BaseConfiguration>()
    .UseMongo()
    .UseGraphQL("/gql", (server, services, builder) =>
    {
        server
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddSubscriptionType<Subscription>();

        server
            .AddType<Use>()
            .AddType<Resume>();
    })
    .Configure((WebApplicationBuilder builder) =>
    {
        builder.Services
            .AddSingleton<IAuthorizationService, AuthorizationService>();

        builder.Services
            .AddSingleton<JwtSecurityTokenHandler>();
    })
    .Build()
    .Run();