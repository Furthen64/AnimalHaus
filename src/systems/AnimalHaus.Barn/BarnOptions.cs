namespace AnimalHaus.Barn;

public sealed class BarnOptions
{
    // Inventory / capacity
    public int InitialFeedUnits { get; set; } = 6;
    public int StorageCapacity { get; set; } = 24;

    // Alerting
    public int ResourceLowThreshold { get; set; } = 2;

    // Dispatch policy
    public int AssignTaskTimeoutMs { get; set; } = 500;

    // Quality control
    public int InitialFreshnessScore { get; set; } = 100;
    public int FreshnessDecayMin { get; set; } = 1;
    public int FreshnessDecayMax { get; set; } = 3;
}
