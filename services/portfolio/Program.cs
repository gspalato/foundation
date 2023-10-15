using Octokit;
using Octokit.Internal;
using Foundation.Common.Configurations;
using Foundation.Core.SDK;
using Foundation.Core.SDK.API.GraphQL;
using Foundation.Core.SDK.Database.Mongo;
using Foundation.Services.Portfolio.Configurations;
using Foundation.Services.Portfolio.Services;
using Foundation.Core.SDK.API.REST;

new ServiceBuilder(args)
    .WithName("Portfolio")
    .BindConfiguration<IBaseConfiguration, Configuration>()
    .BindConfiguration<Configuration>()
    .UseMongo()
    .UseREST(enableSwagger: true)
    .UseGraphQL("/gql", (server, services, builder) =>
    {
        server
            .AddGeneratedQueryType();

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