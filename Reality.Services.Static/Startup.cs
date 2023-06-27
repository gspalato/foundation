using HotChocolate.AspNetCore;
using HotChocolate.Subscriptions;
using Reality.Common.Configurations;
using Reality.Common.Entities;
using Reality.Common.Services;
using Reality.Services.Static.Types;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;

namespace Reality.Services.Static
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

			// GraphQL
			services
				.AddGraphQLServer()
				.AddInMemorySubscriptions()
				.AddQueryType<Query>()
				.AddMutationType<Mutation>()
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
				endpoints.MapGraphQL("/gql")
                    .WithOptions(new GraphQLServerOptions()
                    {
                        Tool = { Enable = false }
                    });
			});

			static string ScanFolder(DirectoryInfo directory, string indentation = "\t", int maxLevel = -1, int deep = 0)
			{
				StringBuilder builder = new StringBuilder();

				builder.AppendLine(string.Concat(Enumerable.Repeat(indentation, deep)) + directory.Name);

				if (maxLevel == -1 || deep < maxLevel )
				{
					foreach (var subdirectory in directory.GetDirectories())
						builder.Append(ScanFolder(subdirectory, indentation, maxLevel, deep + 1));
				}

				foreach (var file in directory.GetFiles())
					builder.AppendLine(string.Concat(Enumerable.Repeat(indentation, deep + 1)) + file.Name);

				return builder.ToString();
			}

			Console.WriteLine(ScanFolder(new DirectoryInfo(env.ContentRootPath)));
		}
	}
}