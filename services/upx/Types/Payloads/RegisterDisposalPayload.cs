namespace Foundation.Services.UPx.Types.Payloads;

public class RegisterDisposalPayload
{
    public required bool Successful { get; set; } = default!;

    public DisposalClaim Disposal { get; set; } = default!;

    public string? Error { get; set; } = default!;
}
