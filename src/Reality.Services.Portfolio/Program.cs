using Octokit;
using Octokit.Internal;
using Reality.SDK;
using Reality.SDK.API.GraphQL;
using Reality.SDK.Database.Mongo;
using Reality.Services.Portfolio.Configurations;
using Reality.Services.Portfolio.Services;
using Reality.Services.Portfolio.Types;
new ServiceBuilder(args)
    .LoadConfiguration<Configuration>()
    .UseMongo()
    .UseGraphQL("/gql", (server, services, builder) =>
    {
        server.AddQueryType<Query>();
        server.AddType<Project>();

        server
            .AddMongoDbFiltering()
            .AddMongoDbPagingProviders()
            .AddMongoDbProjections()
            .AddMongoDbSorting();
    })
    .Configure((WebApplicationBuilder builder) =>
    {
        var githubToken = builder.Configuration.GetValue<string>("GithubToken");
        if (string.IsNullOrEmpty(githubToken))
            throw new Exception("Github token is not set!");

        // Services
        var productInfo = new Octokit.ProductHeaderValue("RealityAPI", "1.0.0");
        var credentials = new Credentials(githubToken, AuthenticationType.Bearer);
        var store = new InMemoryCredentialStore(credentials);

        builder.Services.AddSingleton<GitHubClient>(_ => new GitHubClient(productInfo, store));

        builder.Services.AddHostedService<ProjectService>();
    })
    .Build()
    .Run();

/*
namespace Reality.Services.Portfolio
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