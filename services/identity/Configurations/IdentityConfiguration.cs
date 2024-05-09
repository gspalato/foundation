using Foundation.Common.Configurations;

namespace Foundation.Services.Identity.Configurations;

public interface IIdentityConfiguration : IBaseConfiguration
{
    public string AwsAccessKey { get; set; }
    public string AwsSecretAccessKey { get; set; }
    public string AwsFoundationIdentityProfilePictureBucket { get; set; }
    public string AwsFoundationIdentityProfilePictureUrlFormat { get; set; }
}

public class IdentityConfiguration : BaseConfiguration, IIdentityConfiguration
{
    public string AwsAccessKey { get; set; } = default!;
    public string AwsSecretAccessKey { get; set; } = default!;
    public string AwsFoundationIdentityProfilePictureBucket { get; set; } = default!;
    public string AwsFoundationIdentityProfilePictureUrlFormat { get; set; } = default!;
}