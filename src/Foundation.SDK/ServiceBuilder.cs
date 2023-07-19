using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Foundation.SDK
{
    public class ServiceBuilder
    {
        public IConfiguration Configuration { get; set; }

        private WebApplicationBuilder WebApplicationBuilder { get; set; }
        private WebApplication? WebApplication { get; set; }

        private List<Action<WebApplication>> Actions { get; set; }

        public ServiceBuilder(string[] args)
        {
            WebApplicationBuilder = WebApplication.CreateBuilder(args);

            WebApplicationBuilder.Configuration.AddEnvironmentVariables();
            Configuration = WebApplicationBuilder.Configuration;

            Actions = new();
        }

        /// <summary>
        /// Binds loaded configuration to the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration class.</typeparam>
        /// <returns>The current instance of the <see cref="ServiceBuilder"/> class.</returns>
        public ServiceBuilder BindConfiguration<T>() where T : class, new()
        {
            T config = Activator.CreateInstance<T>();
            WebApplicationBuilder.Configuration.Bind(config);
            WebApplicationBuilder.Services.AddSingleton(config);

            return this;
        }

        /// <summary>
        /// Binds loaded configuration to the specified type, utilizing the specified interface.
        /// </summary>
        /// <typeparam name="I">The type of the interface.</typeparam>
        /// <typeparam name="T">The type of the configuration class.</typeparam>
        /// <returns>The current instance of the <see cref="ServiceBuilder"/> class.</returns>
        public ServiceBuilder BindConfiguration<I, T>()
            where T : class, I, new()
            where I : class
        {
            T config = Activator.CreateInstance<T>();
            WebApplicationBuilder.Configuration.Bind(config);
            WebApplicationBuilder.Services.AddSingleton<I, T>((_) => config);

            BindConfiguration<T>();

            return this;
        }

        public ServiceBuilder Configure(Action<WebApplicationBuilder> configure)
        {
            configure(WebApplicationBuilder);
            return this;
        }

        public ServiceBuilder Configure(Action<WebApplication> configure)
        {
            Actions.Add(configure);
            return this;
        }

        public ServiceBuilder AddCors(string policyName, CorsPolicy policy)
        {

            Configure((WebApplicationBuilder appBuilder) =>
            {
                appBuilder.Services.AddCors(options =>
                {
                    options.AddPolicy(policyName, policy);
                });
            });

            Configure((WebApplication app) =>
            {
                app.UseCors(policyName);
            });

            return this;
        }

        public ServiceBuilder Build()
        {
            var baseConfigurationBuilder = new ConfigurationBuilder()
                .SetBasePath(WebApplicationBuilder!.Environment.ContentRootPath)
                .AddEnvironmentVariables();

            var baseConfiguration = baseConfigurationBuilder.Build();
            Configuration = baseConfiguration;

            WebApplicationBuilder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog(new Foundation.Common.Configurations.LoggingConfiguration());
            });

            WebApplication = WebApplicationBuilder.Build();

            WebApplication.UseRouting();

            foreach (var action in Actions)
                action(WebApplication);

            Actions.Clear();

            return this;
        }

        public void Run() => WebApplication?.Run();
    }
}