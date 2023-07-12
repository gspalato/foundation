using Reality.Common.Entities;

namespace Reality.Common.Payloads
{
    public class CheckAuthenticationPayload
    {
        public bool Successful { get; set; } = false;

        public User? User { get; set; } = default!;
    }
}
