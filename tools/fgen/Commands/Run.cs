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
    public ValueTask ExecuteAsync(IConsole console)
    {
        // Parse solution file containing all microservices.
        DotNetSolution solution;
        try
        {
            solution = DotNetSolution.Load(@"./Foundation.sln");
        }
        catch
        {
            console.Output.WriteLine("Couldn't find Foundation.sln file. Make sure this tool is being ran from the root of the repository.");
            return ValueTask.CompletedTask;
        }

        List<Project> projects = new();
        foreach (var solutionProjectDefinition in solution.Projects)
        {
            var projectDefinition = DotNetProject.Load(solutionProjectDefinition.Path.Replace('\\', '/'));

            var fullPath = Path.Join(Directory.GetCurrentDirectory(), solutionProjectDefinition.Path).Replace('\\', '/');
            var projectFolder = new FileInfo(fullPath).Directory.FullName;
            console.Output.WriteLine($"Loading files for {solutionProjectDefinition.Name} at {projectFolder}...");

            Project project = new()
            {
                Name = solutionProjectDefinition.Name,
                Path = projectFolder,
                Frameworks = projectDefinition.ProjectTargets
            };
            
            // Get files in project folder root.
            var rootFiles = Directory.GetFiles(projectFolder, "*.cs");
            foreach (var file in rootFiles)
            {
                if (file is null || file is "")
                    continue;

                console.Output.WriteLine($"Found file {file}.");

                var sourceFile = new SourceFile
                {
                    Path = file,
                    Content = File.ReadAllText(file)
                };

                project.Files.Add(Guid.NewGuid(), sourceFile);
            }

            // Get all other files, ignoring bin and obj folders.
            var subfolders = Directory.GetDirectories(projectFolder);
            foreach (var subfolder in subfolders)
            {
                if (subfolder.EndsWith("/bin") || subfolder.EndsWith("/obj"))
                    continue;

                var files = Directory.GetFiles(subfolder, "*.cs", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (file is null || file is "")
                    continue;

                    console.Output.WriteLine($"Found file {file}.");

                    var sourceFile = new SourceFile
                    {
                        Path = file,
                        Content = File.ReadAllText(file)
                    };

                    project.Files.Add(Guid.NewGuid(), sourceFile);
                }
            }

            projects.Add(project);
        }

        CodegenService codegenService = new();
        foreach (var project in projects)
            // Generate code for each project.
            codegenService.GenerateForProject(project);

        return ValueTask.CompletedTask;
    }
}