namespace Foundation.Common.Entities;

public class Project : BaseEntity
{
    /// <summary>
    /// The project's name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// The URL to the icon's animated version video for the project.
    /// </summary>
    /// <remarks>
    /// The file is a <b>.webm</b> file.
    /// </remarks>
    public string? AnimatedIconUrl { get; set; } = default!;

    /// <summary>
    /// The URL to the icon image for the project.
    /// </summary>
    /// <remarks>
    /// The file is a <b>.webp</b> file.
    /// </remarks>
    public string? IconUrl { get; set; } = default!;
    /// <summary>
    /// The Fallback URL to the icon image for the project.
    /// </summary>
    /// <remarks>
    /// The file is a <b>.webp</b> file.
    /// </remarks>
    public string? FallbackIconUrl { get; set; } = default!;

    /// <summary>
    /// The URL to the banner's animated version video for the project.
    /// </summary>
    /// <remarks>
    /// The file is a <b>.webm</b> file.
    /// </remarks>
    public string? AnimatedBannerUrl { get; set; } = default!;

    /// <summary>
    /// The URL to the banner image for the project.
    /// </summary>
    /// <remarks>
    /// The file is a <b>.webp</b> file.
    /// </remarks>
    public string? BannerUrl { get; set; } = default!;
    /// <summary>
    /// The Fallback URL to the banner image for the project.
    /// </summary>
    /// <remarks>
    /// The file is a <b>.jpg</b> filee
    /// </remarks>
    public string? FallbackBannerUrl { get; set; } = default!;

    /// <summary>
    /// The project's description.
    /// </summary>
    public string? Description { get; set; } = default!;

    /// <summary>
    /// The project's repository URL on GitHub.
    /// </summary>
    public string? RepositoryUrl { get; set; } = default!;

    /// <summary>
    /// The project's deployment URL, usually for websites.
    /// </summary>
    public string? DeploymentUrl { get; set; } = default!;
}
