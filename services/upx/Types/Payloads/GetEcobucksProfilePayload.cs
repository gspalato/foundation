using Foundation.Core.SDK.API.REST;

namespace Foundation.Services.UPx.Types.Payloads;

public class GetEcobucksProfilePayload : BasePayload
{
    public EcobucksProfile? Profile { get; set; } = default!;
}
