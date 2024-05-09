namespace Foundation.Services.UPx.Types.Payloads;

public class ClaimDisposalAndCreditsInput
{
    public required string UserToken { get; set; }
    
    public required string DisposalToken { get; set; }
}
