﻿using MongoDB.Driver;
using Octokit;
using Reality.Common.Data;
using Reality.Services.Portfolio.Configurations;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Project = Reality.Common.Entities.Project;

namespace Reality.Services.Portfolio.Services;

public class ProjectService : IHostedService, IDisposable
{
    private Configuration Configuration { get; }

    private GitHubClient GitHubClient { get; }
    private ILogger<ProjectService> Logger { get; }

    private readonly IMongoCollection<Project> Projects;

    private int ExecutionCount;
    private Timer? Timer;

    public ProjectService(Configuration configuration, GitHubClient github,
        IDatabaseContext databaseContext, ILogger<ProjectService> logger)
    {
        Configuration = configuration;

        GitHubClient = github;
        Logger = logger;
        Projects = databaseContext.GetCollection<Project>("github_projects");
    }

    public Task StartAsync(CancellationToken token)
    {
        Logger.LogInformation("Project Service started.");

        Timer = new Timer(
            callback: (state) =>
            {
                _ = UpdateProjects(state);
            },
            state: null,
            TimeSpan.Zero,
            period: TimeSpan.FromMinutes(Configuration.ProjectUpdateInterval)
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken token)
    {
        Timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Timer?.Dispose();
    }


    private async Task UpdateProjects(object? state)
    {
        var count = Interlocked.Increment(ref ExecutionCount);

        Logger.LogInformation("Updating projects. Count: {Count}", count);

        var repoCollection = new RepositoryCollection();
        var allRepos = await GitHubClient.Repository.GetAllForCurrent();

        Logger.LogDebug("Found {Count} repositories: {Repos} belonging to {User}",
            allRepos.Count, string.Join(", ", allRepos.Select(x => x.FullName).ToList()), (await GitHubClient.User.Current()).Login);

        foreach (var repo in allRepos)
        {
            try
            {
                Logger.LogDebug("Iterating...");
                if (repo is null)
                {
                    Logger.LogError("Repo is null");
                    continue;
                }

                repoCollection.Add(repo.FullName);

                var projectContents = await GitHubClient.Repository.Content.GetAllContents(repo.Owner.Login, repo.Name, ".project");
                if (projectContents is null || projectContents.Count == 0)
                {
                    Logger.LogDebug("No project metadata found for {Repo}", repo.FullName);
                    continue;
                }

                var metadataFile = projectContents.FirstOrDefault(x => x.Type == ContentType.File && x.Name == "metadata.yml");
                if (metadataFile is null)
                {
                    Logger.LogDebug("No project metadata found for {Repo}", repo.FullName);
                    continue;
                }

                var owner = repo.Owner.Login;
                var path = metadataFile!.Path;

                var raw = await GitHubClient.Repository.Content.GetRawContent(owner, repo.Name, path);
                if (raw is null || raw.Length is 0)
                {
                    Logger.LogError("No content found for {Repo}", repo.Name);
                    continue;
                }

                Logger.LogDebug("Found repo {Repo} with project metadata", repo.Name);

                var content = System.Text.Encoding.UTF8.GetString(raw);
                Logger.LogDebug("Content: {Content}", content);

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                var project = deserializer.Deserialize<Project>(content);

                var icon = projectContents.Where(c => c.Type == ContentType.File && c.Name == "icon.jpg").FirstOrDefault();
                project.IconUrl ??= icon?.DownloadUrl;
                project.RepositoryUrl ??= repo?.HtmlUrl;

                Logger.LogDebug("Deserialized project {Project} with URL {URL}", project.Name, project.RepositoryUrl);

                var filter = Builders<Project>.Filter.Where(x => x.Name == project.Name
                    && x.RepositoryUrl == project.RepositoryUrl);

                var existing = await Projects.FindAsync(filter);
                if (!existing.Any())
                {
                    Logger.LogInformation("Inserting project {Project} into database...", project.Name);
                    await Projects.InsertOneAsync(project);
                    continue;
                }
                else
                {
                    Logger.LogInformation("Found existing project {Project} in database, updating...", project.Name);
                    await Projects.ReplaceOneAsync(filter, project);
                }

                Logger.LogInformation("Updated project {Project} in database", project.Name);
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating project {Repo}", repo.FullName);
            }
        }

        /*
        var possibleFiles = await GitHubClient.Search.SearchCode(new SearchCodeRequest
        {
            Path = "/.project",
            FileName = "metadata.yml",
            Repos = repoCollection,
        });
        

        var projectFiles = possibleFiles.Items.Where(x => x.Name == "metadata.yml");

        Logger.LogDebug("Found {Count} project metadata files: {Files}", projectFiles.Count(), string.Join(", ", projectFiles.Select(x => x.Repository.FullName).ToList()));

        foreach (var metadataFile in projectFiles)
        {
            var owner = metadataFile.Repository.Owner.Login;
            var repo = metadataFile.Repository.Name;
            var path = metadataFile.Path;

            Logger.LogDebug("Found repo {Repo} with project metadata", repo);

            var raw = await GitHubClient.Repository.Content.GetRawContent(owner, repo, path);
            if (raw is null || raw.Length is 0)
            {
                Logger.LogError("No content found for {Repo}", repo);
                continue;
            }

            var content = System.Text.Encoding.UTF8.GetString(raw);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var project = deserializer.Deserialize<Project>(content);

            var repoContent = await GitHubClient.Repository.Content.GetAllContents(owner, repo, "/.project");
            var repository = repoContent.Where(c => c.Type == ContentType.File && c.Name == "icon.jpg").FirstOrDefault();
            project.IconUrl ??= repository?.DownloadUrl;
            project.RepositoryUrl ??= repository?.HtmlUrl;

            Logger.LogDebug("Deserialized project {Project} with URL {URL}", project.Name, project.RepositoryUrl);

            var filter = Builders<Project>.Filter.Where(x => x.Name == project.Name
                && x.RepositoryUrl == project.RepositoryUrl);

            await Projects.ReplaceOneAsync(filter, project, new ReplaceOptions { IsUpsert = true });

            Logger.LogInformation("Updated project {Project} in database", project.Name);
        }
        */
    }
}