using HotChocolate.Language;
using Octokit;
using Octokit.Internal;
using Foundation.Common.Configurations;
using Foundation.SDK;
using Foundation.SDK.API.GraphQL;
using Foundation.SDK.Database.Mongo;
using Foundation.Services.Portfolio.Configurations;
using Foundation.Services.Portfolio.Services;
using Foundation.Services.Portfolio.Types;

new ServiceBuilder(args)
    .BindConfiguration<IBaseConfiguration, Configuration>()
    .BindConfiguration<Configuration>()
    .UseMongo()
    .UseGraphQL("/gql", (server, services, builder) =>
    {
        server
            .AddQueryType<Query>();

        server
            .AddMongoDbFiltering()
            .AddMongoDbPagingProviders()
            .AddMongoDbProjections()
            .AddMongoDbSorting();

        server
            .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);
    })
    .Configure((WebApplicationBuilder builder) =>
    {
        var githubToken = builder.Configuration.GetValue<string>("GithubToken");
        if (string.IsNullOrEmpty(githubToken))
            throw new Exception("Github token is not set!");

        // Services
        var productInfo = new Octokit.ProductHeaderValue("FoundationAPI", "1.0.0");
        var credentials = new Credentials(githubToken, AuthenticationType.Bearer);
        var store = new InMemoryCredentialStore(credentials);

        builder.Services.AddSingleton<GitHubClient>(_ => new GitHubClient(productInfo, store));

        builder.Services.AddHostedService<ProjectService>();
    })
    .Build()
    .Run();

/*
namespace Foundation.Services.Portfolio
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