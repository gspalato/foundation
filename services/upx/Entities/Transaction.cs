using Foundation.Common.Entities;

namespace Foundation.Services.UPx;

public enum TransactionType
{
    Claim,
    Spend
}

public class Transaction : BaseEntity
{
    public required TransactionType TransactionType { get; set; }

    public required string UserId { get; set; } = default!;

    public string ClaimId { get; set; } = default!;

    public required double Credits { get; set; }

    public required long Timestamp { get; set; }

    public string Description { get; set; } = default!;
}