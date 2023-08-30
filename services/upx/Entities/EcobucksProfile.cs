using Foundation.Common.Entities;

namespace Foundation.Services.UPx;

public class EcobucksProfile : BaseEntity
{
    public required string Name { get; set; }

    public required string Username { get; set; }

    public required double Credits { get; set; }

    public required bool IsOperator { get; set; }
}
