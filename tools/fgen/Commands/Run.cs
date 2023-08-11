using Foundation.Tools.Codegen.Services;
using Spectre.Console.Cli;

namespace Foundation.Tools.Codegen.Commands;

public class RunCommand : Command
{
    private CodegenService CodegenService { get; }

    private IOService IOService { get; }

    public RunCommand(CodegenService codegenService, IOService ioService)
    {
        CodegenService = codegenService;
        IOService = ioService;
    }

    public override int Execute(CommandContext context)
    {
        var projects = IOService.GetProjectsFromSolution()!;
        if (projects is null)
            return default;

        foreach (var project in projects)
            CodegenService.GenerateForProject(project);

        return default;
    }
}