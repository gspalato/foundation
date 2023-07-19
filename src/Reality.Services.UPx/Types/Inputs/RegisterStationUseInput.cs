namespace Reality.Services.UPx.Types.Payloads
{
    public class RegisterStationUseInput
    {
        public int StartTimestamp { get; set; }

        public int EndTimestamp { get; set; }

        public int Duration { get; set; }

        public double DistributedWater { get; set; }

        public double EconomizedPlastic { get; set; }

        public double BottleQuantityEquivalent { get; set; }

        public required string Token { get; set; }
    }
}