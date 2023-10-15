using Foundation.Common.Entities;
using Foundation.Core.SDK.API.REST;

namespace Foundation.Services.Identity.Types.Payloads;

public class CheckAuthenticationPayload : BasePayload
{
    public User? User { get; set; } = default!;
}

