using System.IdentityModel.Tokens.Jwt;
using Reality.Common.Entities;
using Reality.Common.Services;
using Reality.SDK;
using Reality.SDK.API.GraphQL;
using Reality.SDK.Database.Mongo;
using Reality.Services.UPx.Types;

new ServiceBuilder(args)
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

/*
namespace Reality.Services.UPx
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
        }
    }
}
*/