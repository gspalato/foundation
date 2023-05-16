using HotChocolate.Types;
using Reality.Common.Entities;

namespace Reality.Services.IoT.UPx.Types
{
    public class ResumeType : ObjectType<Resume>
    {
        protected override void Configure(IObjectTypeDescriptor<Resume> descriptor)
        {
            descriptor.Field(_ => _.Id);
            descriptor.Field(_ => _.Date);
            descriptor.Field(_ => _.TotalDuration);
            descriptor.Field(_ => _.EconomizedWater);
            descriptor.Field(_ => _.EconomizedPlastic);
        }
    }
}