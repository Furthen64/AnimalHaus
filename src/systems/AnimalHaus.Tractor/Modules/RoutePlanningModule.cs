namespace AnimalHaus.Tractor.Modules;

public sealed class RoutePlanningModule
{
    public string Plan(string destinationSystem) => $"barn-to-{destinationSystem.ToLowerInvariant()}";
}
