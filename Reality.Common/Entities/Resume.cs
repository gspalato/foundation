namespace Reality.Common.Entities
{
    public class Resume : BaseEntity
    {
        public string Date { get; set; } = default!;
        public int TotalUses { get; set; } = default!;
        public int TotalDuration { get; set; } = default!;
        public double EconomizedPlastic { get; set; } = default!;
        public double DistributedWater { get; set; } = default!;
        public double BottleQuantityEquivalent { get; set; } = default!;
    }
}
