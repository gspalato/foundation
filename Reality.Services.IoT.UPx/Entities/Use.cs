using Reality.Common.Entities;

namespace Reality.Services.IoT.UPx.Entities
{
    public class Use : BaseEntity
    {
        public int StartTimestamp { get; set; } = default!;
        public int EndTimestamp { get; set; } = default!;
        public int Duration { get; set; } = default!;
        public double EconomizedWater { get; set; } = default!;
        public double EconomizedPlastic { get; set; } = default!;
    }
}
