namespace Foundation.Common.Roles;

public enum Role
{
    [RoleInfo(0)]
    Owner,

    [RoleInfo(1)]
    Microservice,

    [RoleInfo(2)]
    Project,

    [RoleInfo(3)]
    Trusted,

    [RoleInfo(4)]
    User
}

public class RoleInfoAttribute : System.Attribute
{
    public int Priority;

    public RoleInfoAttribute(int priority)
    {
        Priority = priority;
    }
}
public static class RoleEnumExtension
{
    public static string? GetName(this Role role)
    {
        return Enum.GetName(role);
    }

    public static string? GetName(this int n)
    {
        if (Enum.IsDefined(typeof(Role), n))
            return Enum.GetName((Role)n);

        return null;
    }

    public static int? GetPriority(this Role role)
    {
        RoleInfoAttribute[] attributes = (RoleInfoAttribute[])
        role.GetType().GetField(Enum.GetName(role)!)!
        .GetCustomAttributes(typeof(RoleInfoAttribute), false);

        return attributes.Length > 0 ? attributes[0].Priority : null;
    }
}
