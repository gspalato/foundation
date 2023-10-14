using Foundation.Common.Configurations;

namespace Foundation.Services.Identity.Configurations;

public interface IIdentityConfiguration : IBaseConfiguration
{
    public string AwsAccessKey { get; }
    public string AwsSecretAccessKey { get; }
    public string AwsFoundationIdentityProfilePictureBucket { get; }
    public string AwsFoundationIdentityProfilePictureUrlFormat { get; }
}

public class IdentityConfiguration : BaseConfiguration, IIdentityConfiguration
{
    public string AwsAccessKey { get; } = default!;
    public string AwsSecretAccessKey { get; } = default!;
    public string AwsFoundationIdentityProfilePictureBucket { get; } = default!;
    public string AwsFoundationIdentityProfilePictureUrlFormat { get; } = default!;
}