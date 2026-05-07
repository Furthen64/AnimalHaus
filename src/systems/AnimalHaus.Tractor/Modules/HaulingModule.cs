namespace AnimalHaus.Tractor.Modules;

public sealed class HaulingModule
{
    private PendingTask? pendingTask;

    public bool HasPendingTask => pendingTask is not null;

    public void Assign(string taskName, string destinationSystem, string correlationId, string causationId, string route)
    {
        pendingTask = new PendingTask(taskName, destinationSystem, correlationId, causationId, route, 1);
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
