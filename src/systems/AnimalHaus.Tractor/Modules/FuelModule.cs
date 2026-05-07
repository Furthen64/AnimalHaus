namespace AnimalHaus.Tractor.Modules;

public sealed class FuelModule
{
    public int FuelLevel { get; private set; } = 70;

    public bool IsLow => FuelLevel <= 25;

    public void Consume(int amount) => FuelLevel = Math.Max(0, FuelLevel - amount);

    public void Refuel(int amount) => FuelLevel = Math.Min(100, FuelLevel + amount);
}
