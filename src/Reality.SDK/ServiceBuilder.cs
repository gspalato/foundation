using Microsoft.AspNetCore;
using NLog.Extensions.Logging;
using System;

namespace Reality.SDK
{
    public class ServiceBuilder
    {
        public IConfiguration? Configuration { get; set; }

        private WebApplicationBuilder? WebApplicationBuilder { get; set; }
        private WebApplication? WebApplication { get; set; }

        private List<Action<WebApplication>> Actions { get; set; }

        public ServiceBuilder(string[] args)
        {
            WebApplicationBuilder = WebApplication.CreateBuilder(args);

            Actions = new();
        }

        public ServiceBuilder LoadConfiguration<T>() where T : class, new()
        {
            WebApplicationBuilder!.Configuration.AddEnvironmentVariables();

            T config = Activator.CreateInstance<T>();
            WebApplicationBuilder.Configuration.Bind(config);
            WebApplicationBuilder.Services.AddSingleton(config);

            return this;
        }

        public ServiceBuilder Configure(Action<WebApplicationBuilder> configure)
        {
            configure(WebApplicationBuilder!);
            return this;
        }

        public ServiceBuilder Configure(Action<WebApplication> configure)
        {
            Actions.Add(configure);
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
                loggingBuilder.AddNLog(new Reality.Common.Configurations.LoggingConfiguration());
            });

            WebApplication = WebApplicationBuilder.Build();

            WebApplication.UseRouting();

            foreach (var action in Actions)
                action(WebApplication);

            WebApplicationBuilder = null;
            Actions.Clear();

            return this;
        }

        public void Run() => WebApplication?.Run();
    }
}