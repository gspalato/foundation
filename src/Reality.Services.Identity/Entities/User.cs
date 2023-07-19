using Reality.Common.Roles;
using Reality.SDK.Database.Mongo;

namespace Reality.Services.Identity.Entities
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
