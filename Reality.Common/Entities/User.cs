namespace Reality.Common.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public Role[] Roles { get; set; } = default!;
    }

    public class BareUser : BaseEntity
    {
        public string Username { get; set; } = default!;
        public Role[] Roles { get; set; } = default!;
    }
}
