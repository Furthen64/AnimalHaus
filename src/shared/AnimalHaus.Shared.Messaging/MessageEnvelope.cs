namespace AnimalHaus.Shared.Messaging;

public sealed record MessageEnvelope(
    string Topic,
    string MessageType,
    MessageMetadata Metadata,
    string PayloadJson);
