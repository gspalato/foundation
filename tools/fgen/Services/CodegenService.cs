using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foundation.Tools.Codegen.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Foundation.Tools.Codegen.Services;

public class CodegenService
{
    private const string GeneratedRelativePath = "obj/foundation_generated";

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

    /// <summary>
    ///   Generates code for a project.
    ///   Runs all generators on all files in the project.
    ///   Then saves the generated code to the project's obj/foundation_generated folder.
    /// </summary>
    /// <param name="project"></param>
    public void GenerateForProject(Project project)
    {
        Dictionary<Guid, SyntaxTree> trees = new();
        
        foreach (var file in project.Files)
        {
            var tree = CSharpSyntaxTree.ParseText(file.Value.Content);
            trees.Add(file.Key, tree);
        }

        foreach (var tree in trees)
        {
            var file = project.Files[tree.Key];
            foreach (var generatorType in GeneratorTypes)
            {
                var generator = (Generator)Activator.CreateInstance(generatorType);
                foreach (var node in tree.Value.GetRoot().DescendantNodes())
                    generator.OnVisitSyntaxNode(node);
                
                var result = generator.Generate(tree.Value, project);
                if (!result.Success)
                    continue;
                    
                var generatedPath = Path.Combine(project.Path, GeneratedRelativePath);
                Console.WriteLine($"Generated code for {file.Path}:\n{result.Source}\n@ {generatedPath}\n\n\n");
            }
        }
    }
}