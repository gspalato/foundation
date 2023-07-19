using Reality.Common.Entities;

namespace Reality.Services.Identity.Types.Payloads
{
    public class CheckAuthenticationPayload
    {
        public bool Successful { get; set; } = false;

        public User? User { get; set; } = default!;
    }
}
