using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Foundation.Tools.Codegen.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Foundation.Tools.Codegen.Commands;

public class RunCommand : Command<RunCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[SolutionPath]")]
        public string SolutionPath { get; set; } = "./Foundation.sln";
    }


    private CodegenService CodegenService { get; }

    private IOService IOService { get; }

    public RunCommand(CodegenService codegenService, IOService ioService)
    {
        CodegenService = codegenService;
        IOService = ioService;
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var isAbsolutePath = Path.IsPathFullyQualified(settings.SolutionPath);

        var path = Path.GetFullPath(
            isAbsolutePath
                ? settings.SolutionPath
                : Path.Combine(Directory.GetCurrentDirectory(), settings.SolutionPath)
        );

        var isPathFolder = Directory.Exists(path);

        if (isPathFolder)
            path = Path.Combine(path, "Foundation.sln");
        else if (File.Exists(path) && Path.GetExtension(path) != ".sln")
        {
            AnsiConsole.MarkupLine($"{Emoji.Known.CrossMark} [bold red]The path provided is not a solution file.[/]");
            return default;
        }

        Console.WriteLine(path);

        var projects = IOService.GetProjectsFromSolution(path)!;

        CodegenService.LoadProjects(projects);

        if (projects is null)
            return default;

        foreach (var project in projects)
            CodegenService.GenerateForProject(project);

        return default;
    }
}