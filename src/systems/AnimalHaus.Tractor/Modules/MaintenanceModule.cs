using AnimalHaus.Tractor;

namespace AnimalHaus.Tractor.Modules;

public sealed class MaintenanceModule
{
    private readonly int _wearIncreasePerTask;
    private readonly int _maintenanceRecoveryAmount;

    public int WearScore { get; private set; }

    public MaintenanceModule(TractorOptions options)
    {
        WearScore = options.InitialWearScore;
        _wearIncreasePerTask = options.WearIncreasePerTask;
        _maintenanceRecoveryAmount = options.MaintenanceRecoveryAmount;
    }

    public void RecordHaul() => WearScore = Math.Min(100, WearScore + _wearIncreasePerTask);

    public void Schedule() => WearScore = Math.Max(0, WearScore - _maintenanceRecoveryAmount);
}
