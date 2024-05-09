using Foundation.Common.Roles;

namespace Foundation.Common.Entities;

public class Application : BaseEntity
{
    public string Name { get; set; } = default!;
    public Role[] Roles { get; set; } = default!;
}