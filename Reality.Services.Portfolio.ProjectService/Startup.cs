using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Reality.Common;
using Reality.Common.Configurations;
using Reality.Common.Data;
using Reality.Services.Portfolio.ProjectService.Queries;
using Reality.Services.Portfolio.ProjectService.Repositories;
using Reality.Services.Portfolio.ProjectService.Types;
using System.Reflection;

namespace Reality.Services.Portfolio.ProjectService
{
	public class Startup
	{
		public IConfiguration Configuration { get; set; }

		public Startup(IWebHostEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddUserSecrets(Assembly.GetExecutingAssembly())
				.AddEnvironmentVariables();

			Configuration = builder.Build();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			// Configurations
			var config = new BaseConfiguration();
			Configuration.Bind(config);

			services.AddSingleton(config);

			// Database Repositories
			var databaseContext = new DatabaseContext(config);
			services.AddSingleton<IDatabaseContext, DatabaseContext>(_ => databaseContext);

			services
				.AddScoped<IProjectRepository, ProjectRepository>();

			// Hangfire
			services
				.AddHangfire(configuration =>
				{
					var client = databaseContext.GetClient();

					configuration
					.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
					.UseSimpleAssemblyNameTypeSerializer()
					.UseRecommendedSerializerSettings()
					.UseMongoStorage(client, "hangfire", new MongoStorageOptions
					{
						MigrationOptions = new MongoMigrationOptions
						{
							MigrationStrategy = new MigrateMongoMigrationStrategy(),
							BackupStrategy = new CollectionMongoBackupStrategy()
						},
						Prefix = "hangfire.mongo.services.portfolio.projects",
						CheckConnection = true,
						CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
					});
				})
				.AddHangfireServer(serverOptions =>
				{
					serverOptions.ServerName = "reality.hangfire.services.portfolio.projects";
				});

			// GraphQL
			services
				.AddGraphQLServer()
				.AddQueryType<Query>()
				.AddType<ProjectType>()
				.AddMongoDbFiltering()
				.AddMongoDbPagingProviders()
				.AddMongoDbProjections()
				.AddMongoDbSorting();

			// Other services
			services
				.AddHostedService<App>()
				.AddSingleton<IJobService, JobService>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGraphQL("/");
			});
		}
	}
}