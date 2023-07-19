using Foundation.Common.Entities;

namespace Foundation.Services.Identity.Types.Payloads
{
    public class CheckAuthenticationPayload
    {
        public bool Successful { get; set; } = false;

        public User? User { get; set; } = default!;
    }
}
