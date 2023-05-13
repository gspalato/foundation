using Reality.Common.Roles;

namespace Reality.Common.Payloads
{
    public class AuthorizationPayload
    {
        public bool Successful { get; set; } = false;

        public List<Role> Roles { get; set; } = default!;
    }
}
