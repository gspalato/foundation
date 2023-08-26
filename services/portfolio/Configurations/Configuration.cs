using Foundation.Common.Configurations;

namespace Foundation.Services.Portfolio.Configurations;

public class Configuration : BaseConfiguration, IBaseConfiguration
{
    public string GithubToken { get; set; } = default!;

    public int ProjectUpdateInterval { get; set; } = 5;
}