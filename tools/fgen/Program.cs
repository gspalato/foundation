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

using Foundation.Tools.Codegen.Commands;
using Foundation.Tools.Codegen.Services;
using Foundation.Tools.Codegen.Structures.Injection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Foundation.Tools.Codegen;

public static class Program
{
    public static int Main(string[] args)
    {
        AnsiConsole.MarkupLine($"\n{Emoji.Known.PartyPopper} [bold aqua]Welcome to the Foundation Codegen Tool![/]");

        // Register services
        var services = new ServiceCollection();
        services.AddSingleton<CodegenService>();
        services.AddSingleton<IOService>();

        var registrar = new TypeRegistrar(services);

        var app = new CommandApp<RunCommand>(registrar);

        app.Configure(config =>
        {
            config.SetApplicationName("fgen");
            config.AddCommand<RunCommand>("run");
        });

        return app.Run(args);
    }
}