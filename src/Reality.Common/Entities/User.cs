using Reality.Common.Roles;

namespace Reality.Common.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = default!;
        public Role[] Roles { get; set; } = default!;
    }

    public class FullUser : User
    {
        public string PasswordHash { get; set; } = default!;
    }
}
