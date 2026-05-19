namespace AnimalHaus.Tractor;

public sealed class TractorOptions
{
    // Fuel policy
    public int InitialFuelLevel { get; set; } = 70;
    public int LowFuelThreshold { get; set; } = 25;
    public int FuelConsumptionPerTask { get; set; } = 12;
    public int MaxFuelLevel { get; set; } = 100;

    // Maintenance policy
    public int InitialWearScore { get; set; } = 10;
    public int WearIncreasePerTask { get; set; } = 8;
    public int MaintenanceRecoveryAmount { get; set; } = 20;

    // Hauling behaviour
    public int TaskDurationTicks { get; set; } = 1;

    // Routing (destination name → route string; falls back to DefaultRouteTemplate)
    public Dictionary<string, string> Routes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string DefaultRouteTemplate { get; set; } = "barn-to-{0}";
}
