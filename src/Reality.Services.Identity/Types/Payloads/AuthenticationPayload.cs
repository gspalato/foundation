using Reality.Common.Entities;

namespace Reality.Services.Identity.Types.Payloads
{
    public class AuthenticationPayload
    {
        public bool Successful { get; set; } = false;
        public string? Token { get; set; } = null;
        public User? User { get; set; } = null;
        public string? Error { get; set; } = "User was not found.";
    }
}
