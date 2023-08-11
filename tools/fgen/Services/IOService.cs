using System;
using System.Collections.Generic;
using System.IO;
using ByteDev.DotNet.Project;
using ByteDev.DotNet.Solution;
using Foundation.Tools.Codegen.Structures;

namespace Foundation.Tools.Codegen.Services;

public class IOService
{
    public List<Project>? GetProjectsFromSolution()
    {
        // Parse solution file containing all microservices.
        DotNetSolution solution;
        try
        {
            solution = DotNetSolution.Load(@"./Foundation.sln");
        }
        catch
        {
            Console.WriteLine("Couldn't find Foundation.sln file. Make sure this tool is being ran from the root of the repository.");
            return null;
        }

        List<Project> projects = new();
        foreach (var solutionProjectDefinition in solution.Projects)
        {
            var projectDefinition = DotNetProject.Load(solutionProjectDefinition.Path.Replace('\\', '/'));

            var fullPath = Path.Join(Directory.GetCurrentDirectory(), solutionProjectDefinition.Path).Replace('\\', '/');
            var projectFolder = new FileInfo(fullPath).Directory!.FullName;
            Console.WriteLine($"Loading files for {solutionProjectDefinition.Name} at {projectFolder}...");

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

        return projects;
    }

    /// <summary>
    /// Writes a file to disk.
    /// </summary>
    /// <param name="filename">The name of the file to save.</param>
    /// <param name="path">The path to save the file to.</param>
    /// <param name="content">The content of the file.</param>
    /// <returns>True if the file was saved successfully, false otherwise.</returns>
    /// <remarks>
    /// If the file already exists, it will be overwritten.
    /// </remarks>
    public bool WriteFile(string filename, string path, string content)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var fullPath = Path.Combine(path, filename);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        try
        {
            File.WriteAllText(fullPath, content);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to save file {path}:\n{e.Message}");
            return false;
        }
    }
}