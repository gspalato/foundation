/// ======================================================================================================================
/// fgen: Foundation Codegen Tool
///
/// This tool is used to generate boilerplate code for Foundation microservices.
///
/// Usage:
///   fgen [OPTIONS] COMMAND [ARGS]...
///
/// Options:
///   --help             Show this message and exit.
///
/// Commands:
///   compose            Foundation on Compose commands.
///   kubernetes         Foundation on Kubernetes commands.
///
/// Subcommands:
///   compose build      Builds the container images.
///   compose up         Starts Foundation on Compose mode.
///   compose down        Stops Foundation on Compose mode and deletes all containers.
///
///   kubernetes build   Builds the container images.
///   kubernetes up      Starts Foundation on Kubernetes mode.
///   kubernetes down    Stops Foundation on Kubernetes mode and deletes all services, deployments and pods.
/// ======================================================================================================================

using CliFx;
using Foundation.Tools.Codegen.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Foundation.Tools.Codegen;

public static class Program
{
    public static async Task<int> Main()
    {
        return await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(commandTypes =>
            {
                var services = new ServiceCollection();

                // Register services
                services.AddSingleton<CodegenService>();

                // Register commands
                foreach (var commandType in commandTypes)
                    services.AddTransient(commandType);

                return services.BuildServiceProvider();
            })
            .Build()
            .RunAsync();
    }
}