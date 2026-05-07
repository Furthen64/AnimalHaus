using AnimalHaus.Shared.Utils;

namespace AnimalHaus.Shared.Messaging;

public static class EnvelopeFactory
{
    public static MessageEnvelope Create<T>(string topic, T payload, string correlationId, string? causationId = null)
    {
        return new MessageEnvelope(
            topic,
            typeof(T).Name,
            new MessageMetadata(Guid.NewGuid(), DateTime.UtcNow, correlationId, causationId, TopicNames.SchemaVersion),
            JsonMessageSerializer.Serialize(payload));
    }
}
