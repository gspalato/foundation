using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foundation.Tools.Codegen.Generators;
using Foundation.Tools.Codegen.Structures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Foundation.Tools.Codegen.Services;

public class CodegenService
{
    private const string GeneratedFolderName = "fgen_generated";

    private List<Type> GeneratorTypes { get; } = new();

    public CodegenService()
    {
        RegisterGenerators();
    }

    private void RegisterGenerators()
    {
        var baseGeneratorType = typeof(Generator);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(baseGeneratorType));

        foreach (var t in types)
            GeneratorTypes.Add(t);
    }

    public static Dictionary<Guid, SyntaxTree> GetAllSyntaxTreesFromProject(Project project)
    {
        var trees = new Dictionary<Guid, SyntaxTree>();
        foreach (var file in project.Files)
        {
            var tree = CSharpSyntaxTree.ParseText(file.Value.Content);
            trees.Add(file.Key, tree);
        }

        return trees;
    }

    /// <summary>
    ///   Generates code for a project.
    ///   Runs all generators on all files in the project.
    ///   Then saves the generated code to the project's obj/foundation_generated folder.
    /// </summary>
    /// <param name="project"></param>
    public void GenerateForProject(Project project)
    {
        Dictionary<Guid, SyntaxTree> trees = GetAllSyntaxTreesFromProject(project);

        foreach (var tree in trees)
        {
            var file = project.Files[tree.Key];
            foreach (var generatorType in GeneratorTypes)
            {
                var generator = (Generator)Activator.CreateInstance(generatorType);
                foreach (var node in tree.Value.GetRoot().DescendantNodes())
                    generator.OnVisitSyntaxNode(node);
                
                var result = generator.Generate(tree.Value, file, project);
                if (!result.Success)
                    continue;
                
                var generatedPath = Path.Combine(project.Path, "obj", "[TargetFramework]", GeneratedFolderName);
                Console.WriteLine($"Generated code for {file.Path} at {generatedPath}.");

                var filename = $"{result.ExpectedFilename ?? Path.GetFileNameWithoutExtension(file.Name)}.g.cs";

                // Create a new file in the obj/Debug and obj/Release folders for each target framework.
                foreach (var target in project.Frameworks)
                {
                    var debugPath = Path.Combine(project.Path, "obj", "Debug", target.Moniker, GeneratedFolderName);
                    var releasePath = Path.Combine(project.Path, "obj", "Release", target.Moniker, GeneratedFolderName);

                    SaveFile(filename, debugPath, result.Source);
                    SaveFile(filename, releasePath, result.Source);
                }
            }
        }
    }

    private static bool SaveFile(string filename, string path, string content)
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