using Reality.Common.Configurations;

namespace Reality.Services.Identity
{
    public class Configuration : BaseConfiguration
    {
        public string AwsAccessKeyId { get; set; } = default!;
        public string AwsSecretAccessKey { get; set; } = default!;
    }
}