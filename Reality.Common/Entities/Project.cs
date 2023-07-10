namespace Reality.Common.Entities
{
    public class Project : BaseEntity
    {
        public string Name { get; set; } = default!;

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

        public string? Description { get; set; } = default!;

        public string? RepositoryUrl { get; set; } = default!;

        public string? DeploymentUrl { get; set; } = default!;
    }
}
