using Foundation.Core.SDK.API.REST;

namespace Foundation.Services.UPx.Types.Payloads;

public class UserDisposalsPayload : BasePayload
{
    public List<DisposalClaim> UserDisposals { get; set; } = default!;
}
