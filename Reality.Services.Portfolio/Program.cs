using Octokit;
using NLog.Extensions.Logging;
using Octokit.Internal;
using Reality.Common.Configurations;
using Reality.Common.Data;
using Reality.Services.Portfolio.Configurations;
using Reality.Services.Portfolio.Repositories;
using Reality.Services.Portfolio.Types;
using Project = Reality.Common.Entities.Project;
using Reality.Services.Portfolio.Services;

var builder = WebApplication.CreateBuilder(args);

var config = new Configuration();
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.Bind(config);
builder.Services.AddSingleton(config);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
    loggingBuilder.AddNLog(new LoggingConfiguration());
});

var databaseContext = new DatabaseContext(config);
builder.Services.AddSingleton<IDatabaseContext, DatabaseContext>(_ => databaseContext);

builder.Services.AddSingleton<IProjectRepository, ProjectRepository>();

var productInfo = new ProductHeaderValue("Reality API", "1.0.0");
var credentials = new Credentials(config.GithubToken, AuthenticationType.Bearer);
var store = new InMemoryCredentialStore(credentials);

builder.Services.AddSingleton<GitHubClient>(_ => new GitHubClient(productInfo, store));
builder.Services.AddSingleton<Connection>(_ => new Connection(productInfo, credentialStore: store));

builder.Services.AddHostedService<ProjectService>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddType<Project>();

// Build app.
var app = builder.Build();

app.MapGraphQL("/gql");
app.Run();