namespace Reality.Common.Entities
{
    public class Resume : BaseEntity
    {
        public string Date { get; set; } = default!;
        public int TotalDuration { get; set; } = default!;
        public double EconomizedPlastic { get; set; } = default!;
        public double EconomizedWater { get; set; } = default!;
    }
}
