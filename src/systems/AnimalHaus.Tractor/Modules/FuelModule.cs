using AnimalHaus.Tractor;

namespace AnimalHaus.Tractor.Modules;

public sealed class FuelModule
{
    private readonly int _lowFuelThreshold;
    private readonly int _maxFuelLevel;

    public int FuelLevel { get; private set; }

    public bool IsLow => FuelLevel <= _lowFuelThreshold;

    public FuelModule(TractorOptions options)
    {
        FuelLevel = options.InitialFuelLevel;
        _lowFuelThreshold = options.LowFuelThreshold;
        _maxFuelLevel = options.MaxFuelLevel;
    }

    public void Consume(int amount) => FuelLevel = Math.Max(0, FuelLevel - amount);

    public void Refuel(int amount) => FuelLevel = Math.Min(_maxFuelLevel, FuelLevel + amount);
}
