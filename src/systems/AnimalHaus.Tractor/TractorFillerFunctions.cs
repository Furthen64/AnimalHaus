namespace AnimalHaus.Tractor;

public static class TractorFillerFunctions
{
    public static int BuildTaskReadiness(int fuelLevel) => ApplyMaintenanceBuffer(NormalizeFuel(fuelLevel));

    public static int NormalizeFuel(int fuelLevel) => Math.Clamp(fuelLevel, 0, 100);

    public static int ApplyMaintenanceBuffer(int fuelLevel) => Math.Max(0, fuelLevel - MaintenanceBuffer());

    private static int MaintenanceBuffer() => 10;
}
