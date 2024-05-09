namespace Foundation.Common;

[AttributeUsage(AttributeTargets.Class)]
public class GeneratedAttribute : Attribute
{
    public string[] Pipelines { get; set; }

    public GeneratedAttribute(params string[] pipelines)
    {
        Pipelines = pipelines;
    }
}