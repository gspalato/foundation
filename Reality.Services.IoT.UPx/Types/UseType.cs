using HotChocolate.Types;
using Reality.Common.Entities;

namespace Reality.Services.IoT.UPx.Types
{
    public class UseType : ObjectType<Use>
    {
        protected override void Configure(IObjectTypeDescriptor<Use> descriptor)
        {
            descriptor.Field(_ => _.Id);
            descriptor.Field(_ => _.StartTimestamp);
            descriptor.Field(_ => _.EndTimestamp);
            descriptor.Field(_ => _.Duration);
            descriptor.Field(_ => _.EconomizedWater);
            descriptor.Field(_ => _.EconomizedPlastic);
            descriptor.Field(_ => _.UsedWater);
        }
    }
}