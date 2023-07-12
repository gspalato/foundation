namespace Reality.Common.Entities
{
    public class BlogPost : BaseEntity
    {
        public string Title { get; set; } = default!;
        public string Headline { get; set; } = default!;
        public string Body { get; set; } = default!;
    }
}