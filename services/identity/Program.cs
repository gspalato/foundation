using Microsoft.AspNetCore.Identity;
using Foundation.Common.Configurations;
using Foundation.Core.SDK;
using Foundation.Core.SDK.API.GraphQL;
using Foundation.Core.SDK.API.REST;
using Foundation.Core.SDK.Auth.JWT;
using Foundation.Core.SDK.Database.Mongo;
using Foundation.Services.Identity.Services;
using Foundation.Services.Identity.Types;
using Foundation.Services.Identity.Configurations;
using Amazon.S3;

new ServiceBuilder(args)
    .WithName("Identity")
    .BindConfiguration<IBaseConfiguration, BaseConfiguration>()
    .BindConfiguration<IIdentityConfiguration, IdentityConfiguration>()
    .UseMongo()
    .UseREST(enableSwagger: true)
    .UseGraphQL("/gql", (server, services, builder) =>
    {
        server.AddGeneratedQueryType();
        server.AddMutationType<Mutation>();
        server.AddType<UploadType>();

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

        appBuilder.Services
            .AddSingleton<IAmazonS3>((provider) =>
            {
                var config = provider.GetRequiredService<IIdentityConfiguration>();

                return new AmazonS3Client(config.AwsAccessKey, config.AwsSecretAccessKey, Amazon.RegionEndpoint.USEast1);
            });
    })
    .Build()
    .Run();