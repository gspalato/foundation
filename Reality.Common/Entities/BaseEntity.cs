using System.Text.Json.Serialization;

namespace Reality.Common.Entities
{
    public class BaseEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
    }
}