using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Reality.Common;
using Reality.Common.Configurations;
using Reality.Common.Data;
using Reality.Common.Services;
using Reality.Services.IoT.UPx.Mutations;
using Reality.Services.IoT.UPx.Queries;
using Reality.Services.IoT.UPx.Repositories;
using Reality.Services.IoT.UPx.Types;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

namespace Reality.Services.IoT.UPx
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
				.AddScoped<IUseRepository, UseRepository>();

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
						Prefix = "hangfire.mongo.services.iot.upx",
						CheckConnection = true,
						CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
					});
				})
				.AddHangfireServer(serverOptions =>
				{
					serverOptions.ServerName = "reality.hangfire.services.iot.upx";
				});

			// GraphQL
			services
				.AddGraphQLServer()
				.AddQueryType<Query>()
				.AddMutationType<Mutation>()
				.AddType<UseType>()
				.ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
				.AddMongoDbFiltering()
				.AddMongoDbPagingProviders()
				.AddMongoDbProjections()
				.AddMongoDbSorting();

			services
				.AddSingleton<IAuthorizationService, AuthorizationService>();

			services
				.AddSingleton<JwtSecurityTokenHandler>();
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