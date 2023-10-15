using Foundation.Common.Entities;
using Foundation.Core.SDK.API.REST;

namespace Foundation.Services.Identity.Types.Payloads;

public class AuthenticationPayload : BasePayload
{
    public string? Token { get; set; } = null;
    public User? User { get; set; } = null;
    public new string? Error { get; set; } = "User was not found.";
}

