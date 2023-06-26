using System.Text.Json.Serialization;
using Reality.Common.Roles;

namespace Reality.Common.Entities
{
    public class User : BaseEntity
    {
        [JsonPropertyName("Username")]
        public string Username { get; set; } = default!;

        [JsonPropertyName("Roles")]
        public Role[] Roles { get; set; } = default!;
    }

    public class FullUser : User
    {
        [JsonPropertyName("PasswordHash")]
        public string PasswordHash { get; set; } = default!;
    }
}
