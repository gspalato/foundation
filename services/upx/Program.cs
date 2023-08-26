using Foundation.Common.Configurations;
using Foundation.Common.Entities;
using Foundation.Core.SDK;
using Foundation.Core.SDK.API.GraphQL;
using Foundation.Core.SDK.Auth.JWT;
using Foundation.Core.SDK.Database.Mongo;
using Foundation.Services.UPx.Types;

new ServiceBuilder(args)
    .WithName("UPx")
    .BindConfiguration<IBaseConfiguration, BaseConfiguration>()
    .UseMongo()
    .UseGraphQL("/gql", (server, services, builder) =>
    {
        server
            .AddGeneratedQueryType()
            .AddMutationType<Mutation>()
            .AddSubscriptionType<Subscription>();

        server
            .AddType<Use>()
            .AddType<Resume>();
    })
    .UseJWT()
    .Build()
    .Run();