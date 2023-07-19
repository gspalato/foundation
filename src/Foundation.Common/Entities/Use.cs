namespace Foundation.Common.Entities
{
    public class Use : BaseEntity
    {
        public int StartTimestamp { get; set; } = default!;
        public int EndTimestamp { get; set; } = default!;
        public int Duration { get; set; } = default!;
        public double EconomizedPlastic { get; set; } = default!;
        public double DistributedWater { get; set; } = default!;
        public double BottleQuantityEquivalent { get; set; } = default!;
    }
}
