namespace AnimalHaus.Pigpen;

public sealed class PigpenOptions
{
    // Initial pig state
    public int InitialWeight { get; set; } = 100;
    public int InitialAgeTicks { get; set; } = 1;
    public int InitialHealthScore { get; set; } = 80;

    // Feeding behaviour
    public int InitialFeedStock { get; set; } = 0;
    public int InitialHungerScore { get; set; } = 45;
    public int HungerIncreasePerTick { get; set; } = 10;
    public int HungerDecreasePerFeed { get; set; } = 20;
    public int FeedPerTick { get; set; } = 1;

    // Growth / transfer
    public int WeightGainFed { get; set; } = 12;
    public int WeightGainUnfed { get; set; } = 4;
    public int TransferWeightThreshold { get; set; } = 140;

    // Pen environment
    public int InitialCleanliness { get; set; } = 88;
    public int CleanlinessDecayMin { get; set; } = 1;
    public int CleanlinessDecayMax { get; set; } = 3;
    public int InitialTemperature { get; set; } = 21;
    public int TemperatureDeltaMin { get; set; } = -1;
    public int TemperatureDeltaMax { get; set; } = 1;

    // Dispatch behaviour
    public int FeedRequestQuantity { get; set; } = 2;
    public int FeedRequestEarliestTick { get; set; } = 2;
}
