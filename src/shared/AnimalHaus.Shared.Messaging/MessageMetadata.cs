namespace AnimalHaus.Shared.Messaging;

public sealed record MessageMetadata(
    Guid MessageId,
    DateTime TimestampUtc,
    string CorrelationId,
    string? CausationId,
    string SchemaVersion);
