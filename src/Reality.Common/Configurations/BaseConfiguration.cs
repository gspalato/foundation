namespace Reality.Common.Configurations
{
    public interface IBaseConfiguration
    {
        string DatabaseHost { get; }
        string DatabaseName { get; }
        string DatabaseUser { get; }
        string DatabasePassword { get; }
    }

    public class BaseConfiguration : IBaseConfiguration
    {
        public string DatabaseHost { get; set; } = default!;
        public string DatabaseName { get; set; } = default!;
        public string DatabaseUser { get; set; } = default!;
        public string DatabasePassword { get; set; } = default!;
    }
}
