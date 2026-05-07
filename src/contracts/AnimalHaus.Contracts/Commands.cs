namespace AnimalHaus.Contracts.Commands;

public sealed record DeliverFeed(string PigpenId, int Quantity);
public sealed record TransferPig(string PigId, string Destination);
public sealed record ReserveResource(string ResourceName, int Quantity);
public sealed record ReleaseResource(string ResourceName, int Quantity);
public sealed record RequestDispatch(string ResourceName, int Quantity, string DestinationSystem);
public sealed record AssignTask(string TaskName, string DestinationSystem, string ResourceName, int Quantity);
public sealed record RefuelTractor(string TractorId, int Quantity);
public sealed record ScheduleMaintenance(string TractorId, string Reason);
public sealed record CommandAccepted(bool Accepted, string Message);
