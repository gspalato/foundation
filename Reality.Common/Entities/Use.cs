using System.Text.Json.Serialization;

namespace Reality.Common.Entities
{
    public class Use : BaseEntity
    {
        [JsonPropertyName("StartTimestamp")]
        public int StartTimestamp { get; set; } = default!;

        [JsonPropertyName("EndTimestamp")]
        public int EndTimestamp { get; set; } = default!;

        [JsonPropertyName("Duration")]
        public int Duration { get; set; } = default!;

        [JsonPropertyName("EconomizedPlastic")]
        public double EconomizedPlastic { get; set; } = default!;

        [JsonPropertyName("DistributedWater")]
        public double DistributedWater { get; set; } = default!;

        [JsonPropertyName("BottleQuantityEquivalent")]
        public double BottleQuantityEquivalent { get; set; } = default!;
    }
}
