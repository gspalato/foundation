namespace Foundation.Services.UPx.Types.Payloads;

public class ClaimDisposalAndCreditsPayload
{
    public required bool Successful { get; set; }

    public string? Error { get; set; }

    public DisposalClaim? Disposal { get; set; }
}
