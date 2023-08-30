using Foundation.Common.Entities;
using Foundation.Common.Roles;

namespace Foundation.Services.UPx;

public class DisposalClaim : BaseEntity
{
    public string? UserId { get; set; } = default!;

    public required string OperatorId { get; set; }

    public required string Token { get; set; }

    public double Credits => Disposals.Sum(x => x.Credits);

    public required bool IsClaimed { get; set; }

    public float Weight => Disposals.Sum(x => x.Weight);

    public required Disposal[] Disposals { get; set; }
}
