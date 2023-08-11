using ByteDev.DotNet;
using ByteDev.DotNet.Project;
using ByteDev.DotNet.Solution;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Foundation.Tools.Codegen.Services;
using Foundation.Tools.Codegen.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Foundation.Tools.Codegen.Commands;

[Command(Description = "Generates boilerplate code for all Foundation microservices.")]
public class RunCommand : ICommand
{
    private CodegenService CodegenService { get; }

    private IOService IOService { get; }

    public RunCommand(CodegenService codegenService, IOService ioService)
    {
        CodegenService = codegenService;
        IOService = ioService;
    }

    public ValueTask ExecuteAsync(IConsole console)
    {
        List<Project> projects = IOService.GetProjectsFromSolution()!;
        if (projects is null)
            return ValueTask.CompletedTask;

        foreach (var project in projects)
            // Generate code for each project.
            CodegenService.GenerateForProject(project);

        return ValueTask.CompletedTask;
    }
}