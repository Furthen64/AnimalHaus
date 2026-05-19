namespace AnimalHaus.MarketPlace;

public sealed class MarketPlaceOptions
{
    // Commodity list with starting prices
    public Dictionary<string, decimal> Commodities { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["eggs"] = 3.10m,
        ["milk"] = 2.45m,
    };

    // Pricing rules
    public int PriceDeltaMin { get; set; } = -6;
    public int PriceDeltaMax { get; set; } = 6;
    public decimal MinPrice { get; set; } = 0.50m;

    // Global currency
    public string Currency { get; set; } = "USD";

    // Publish cadence (publish every N ticks)
    public int PriceUpdateEveryNTicks { get; set; } = 1;
}
