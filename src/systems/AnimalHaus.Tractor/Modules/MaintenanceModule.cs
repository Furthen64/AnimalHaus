namespace AnimalHaus.Tractor.Modules;

public sealed class MaintenanceModule
{
    public int WearScore { get; private set; } = 10;

    public void RecordHaul() => WearScore = Math.Min(100, WearScore + 8);

    public void Schedule() => WearScore = Math.Max(0, WearScore - 20);
}
