using Reality.Common.Entities;

namespace Reality.Services.Database.Types
{
    public class BlogPostType : ObjectType<BlogPost>
    {
        protected override void Configure(IObjectTypeDescriptor<BlogPost> descriptor)
        {
            descriptor.Field(_ => _.Id);
            descriptor.Field(_ => _.Title);
            descriptor.Field(_ => _.Headline);
            descriptor.Field(_ => _.Body);
        }
    }
}