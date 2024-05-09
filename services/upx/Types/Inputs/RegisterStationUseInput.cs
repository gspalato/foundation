namespace Foundation.Services.UPx.Types.Payloads;

public class RegisterStationUseInput
{
    public required int StartTimestamp { get; set; }

    public required int EndTimestamp { get; set; }

    public required int Duration { get; set; }

    public required double DistributedWater { get; set; }

    public required double EconomizedPlastic { get; set; }

    public required double BottleQuantityEquivalent { get; set; }

    public required string Token { get; set; }
}
