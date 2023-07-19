using Reality.Services.Identity.Entities;

namespace Reality.Services.Identity.Payloads
{
    public class CheckAuthenticationPayload
    {
        public bool Successful { get; set; } = false;

        public User? User { get; set; } = default!;
    }
}
