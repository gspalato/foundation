using System;
using System.Collections.Generic;

namespace Foundation.Tools.Codegen.Structures
{
    public interface IGenerationPipeline
    {
        string Name { get; set; }
        
        List<Type> GeneratorTypes { get; set; }
    }

    public class GenerationPipeline : IGenerationPipeline
    {
        public string Name { get; set; } = default!;
        
        public List<Type> GeneratorTypes { get; set; } = new();
    }

    public class GenerationPipelineBuilder
    {
        private GenerationPipeline Pipeline { get; } = new();

        public GenerationPipelineBuilder() { }

        public GenerationPipelineBuilder(string name)
        {
            Pipeline.Name = name;
        }

        public GenerationPipelineBuilder WithName(string name)
        {
            Pipeline.Name = name;
            return this;
        }

        public GenerationPipelineBuilder AddGenerator<T>()
        {
            Pipeline.GeneratorTypes.Add(typeof(T));
            return this;
        }

        public GenerationPipeline Build()
        {
            return Pipeline;
        }
    }
}