using HotChocolate.Types;
using Reality.Common.Entities;

namespace Reality.Services.Portfolio.ProjectService.Types
{
    public class ProjectType : ObjectType<Project>
    {
        protected override void Configure(IObjectTypeDescriptor<Project> descriptor)
        {
            descriptor.Field(_ => _.Id);
            descriptor.Field(_ => _.Name);
            descriptor.Field(_ => _.Description);
            descriptor.Field(_ => _.IconUrl);
            descriptor.Field(_ => _.Url);
        }
    }
}