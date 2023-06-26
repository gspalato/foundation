namespace Reality.Common.Configurations
{
    public interface IBaseConfiguration
    {
        string AwsAccessKeyId { get; set; }
        string AwsSecretAccessKey { get; set; }
    }

    public class BaseConfiguration : IBaseConfiguration
    {
        public string AwsAccessKeyId { get; set; } = default!;

        public string AwsSecretAccessKey { get; set; } = default!;
    }
}
