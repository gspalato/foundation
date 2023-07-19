using MongoDB.Driver;
using Octokit;
using Reality.SDK.Database.Mongo;
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

    private readonly IRepository<Project> ProjectRepository;

    private int ExecutionCount;
    private Timer? Timer;

    public ProjectService(Configuration configuration, GitHubClient github,
        IRepository<Project> projectRepository, ILogger<ProjectService> logger)
    {
        Configuration = configuration;

        GitHubClient = github;
        Logger = logger;
        ProjectRepository = projectRepository;
    }

    public Task StartAsync(CancellationToken token)
    {
        Logger.LogInformation("Project Service started.");

        Timer = new Timer(
            callback: (state) =>
            {
                _ = UpdateProjects();
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


    /// <summary>
    /// Updates the projects in the database.
    /// Fetches all repositories from the GitHub API and checks for a .project folder.
    /// If found, it will parse the .project/metadata.yml file and create a Project entity.
    /// The Project entity is then inserted into the database.
    /// </summary>
    private async Task UpdateProjects()
    {
        var count = Interlocked.Increment(ref ExecutionCount);

        Logger.LogInformation("Updating projects. Count: {Count}", count);

        var allRepos = await GitHubClient.Repository.GetAllForCurrent();

        Logger.LogDebug("Found {Count} repositories: {Repos} belonging to {User}",
            allRepos.Count, string.Join(", ", allRepos.Select(x => x.FullName).ToList()), (await GitHubClient.User.Current()).Login);

        var animatedExtension = ".mp4";
        var defaultExtension = ".webp";
        var fallbackExtension = ".jpg";

        foreach (var repo in allRepos)
        {
            try
            {
                IReadOnlyList<RepositoryContent>? projectContents = null;
                try
                {
                    projectContents = await GitHubClient.Repository.Content.GetAllContents(repo.Owner.Login, repo.Name, ".project");
                }
                catch (Exception ex)
                {
                    if (ex is not Octokit.NotFoundException)
                        Logger.LogError(ex, "Error getting project metadata folder contents for {Repo}.", repo.FullName);

                    continue;
                }

                var metadataFile = projectContents.FirstOrDefault(x => x.Type == ContentType.File && x.Name == "metadata.yml");
                if (metadataFile is null)
                {
                    Logger.LogDebug("No project metadata found for {Repo}.", repo.FullName);
                    continue;
                }

                var owner = repo.Owner.Login;
                var path = metadataFile!.Path;

                var raw = await GitHubClient.Repository.Content.GetRawContent(owner, repo.Name, path);
                if (raw is null || raw.Length is 0)
                {
                    Logger.LogError("No content found for {Repo}.", repo.Name);
                    continue;
                }

                var content = System.Text.Encoding.UTF8.GetString(raw);

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                var project = deserializer.Deserialize<Project>(content);
                if (project.Name is null)
                {
                    Logger.LogError("Project metadata for {Repo} is invalid.", repo.Name);
                    continue;
                }

                var icon = projectContents.Where(c => c.Type == ContentType.File
                    && c.Name == "icon" + defaultExtension).FirstOrDefault();
                project.IconUrl ??= icon?.DownloadUrl;

                var animatedIcon = projectContents.Where(c => c.Type == ContentType.File
                    && c.Name == "icon" + animatedExtension).FirstOrDefault();
                project.AnimatedIconUrl ??= animatedIcon?.DownloadUrl;

                var fallbackIcon = projectContents.Where(c => c.Type == ContentType.File
                    && c.Name == "icon" + fallbackExtension).FirstOrDefault();
                project.FallbackIconUrl ??= fallbackIcon?.DownloadUrl;

                project.IconUrl ??= project.FallbackIconUrl;

                var banner = projectContents.Where(c => c.Type == ContentType.File
                    && c.Name == "banner" + defaultExtension).FirstOrDefault();
                project.RepositoryUrl ??= repo?.HtmlUrl;

                var animatedBanner = projectContents.Where(c => c.Type == ContentType.File
                    && c.Name == "banner" + animatedExtension).FirstOrDefault();
                project.BannerUrl ??= animatedBanner?.DownloadUrl;

                var fallbackBanner = projectContents.Where(c => c.Type == ContentType.File
                    && c.Name == "banner" + fallbackExtension).FirstOrDefault();
                project.FallbackBannerUrl ??= fallbackBanner?.DownloadUrl;

                project.BannerUrl ??= project.FallbackBannerUrl;

                Logger.LogDebug("Deserialized project {Project} with URL {URL}", project.Name, project.RepositoryUrl);

                var filter = Builders<Project>.Filter.Where(x => x.Name == project.Name
                    && x.RepositoryUrl == project.RepositoryUrl);

                var found = await ProjectRepository.Collection.FindAsync(filter);
                if (!found.Any())
                {
                    await ProjectRepository.InsertAsync(project);
                    Logger.LogInformation("Inserted project {Project} into database.", project.Name);
                    continue;
                }
                else
                {
                    var update = Builders<Project>.Update
                        .Set(x => x.Name, project.Name)
                        .Set(x => x.Description, project.Description)
                        .Set(x => x.IconUrl, project.IconUrl)
                        .Set(x => x.FallbackIconUrl, project.FallbackIconUrl)
                        .Set(x => x.BannerUrl, project.BannerUrl)
                        .Set(x => x.FallbackBannerUrl, project.FallbackBannerUrl)
                        .Set(x => x.RepositoryUrl, project.RepositoryUrl)
                        .Set(x => x.DeploymentUrl, project.DeploymentUrl);

                    await ProjectRepository.Collection.UpdateOneAsync(filter, update);

                    Logger.LogInformation("Updated existing project {Project} in database.", project.Name);
                }
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating project {Repo}", repo.FullName);
            }
        }
    }
}