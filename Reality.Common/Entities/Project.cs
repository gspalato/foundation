namespace Reality.Common.Entities
{
    public class Project : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string IconUrl { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Url { get; set; } = default!;
    }
}
