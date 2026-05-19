using AnimalHaus.Tractor;

namespace AnimalHaus.Tractor.Modules;

public sealed class HaulingModule
{
    private readonly int _taskDurationTicks;
    private PendingTask? pendingTask;

    public bool HasPendingTask => pendingTask is not null;

    public HaulingModule(TractorOptions options)
    {
        _taskDurationTicks = options.TaskDurationTicks;
    }

    public void Assign(string taskName, string destinationSystem, string correlationId, string causationId, string route)
    {
        pendingTask = new PendingTask(taskName, destinationSystem, correlationId, causationId, route, _taskDurationTicks);
    }

    public bool TryCompleteTick(out PendingTask completedTask)
    {
        completedTask = default!;
        if (pendingTask is null)
        {
            return false;
        }

        pendingTask = pendingTask with { RemainingTicks = pendingTask.RemainingTicks - 1 };
        if (pendingTask.RemainingTicks > 0)
        {
            return false;
        }

        completedTask = pendingTask;
        pendingTask = null;
        return true;
    }

    public sealed record PendingTask(string TaskName, string DestinationSystem, string CorrelationId, string CausationId, string Route, int RemainingTicks);
}
