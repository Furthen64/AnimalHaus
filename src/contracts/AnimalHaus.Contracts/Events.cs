namespace AnimalHaus.Contracts.Events;

public sealed record PigFed(string PigId, int FeedAmount, int TickNumber);
public sealed record PigHealthChanged(string PigId, int HealthScore, int TickNumber);
public sealed record PigReadyForTransfer(string PigId, int Weight, int TickNumber);
public sealed record InventoryChanged(string ResourceName, int QuantityAvailable, int TickNumber);
public sealed record ResourceLow(string ResourceName, int QuantityAvailable, int TickNumber);
public sealed record DispatchCompleted(string ResourceName, int Quantity, string DestinationSystem, int TickNumber);
public sealed record TractorDispatched(string TractorId, string TaskName, string DestinationSystem, int TickNumber);
public sealed record FuelLow(string TractorId, int FuelLevel, int TickNumber);
public sealed record TaskCompleted(string TractorId, string TaskName, string DestinationSystem, int TickNumber);
