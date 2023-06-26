using System.Text.Json.Serialization;

namespace Reality.Common.Entities
{
    public class Resume : BaseEntity
    {
        [JsonPropertyName("Date")]
        public string Date { get; set; } = default!;

        [JsonPropertyName("TotalDuration")]
        public int TotalDuration { get; set; } = default!;

        [JsonPropertyName("EconomizedPlastic")]
        public double EconomizedPlastic { get; set; } = default!;

        [JsonPropertyName("DistributedWater")]
        public double DistributedWater { get; set; } = default!;

        [JsonPropertyName("BottleQuantityEquivalent")]
        public double BottleQuantityEquivalent { get; set; } = default!;
    }
}
