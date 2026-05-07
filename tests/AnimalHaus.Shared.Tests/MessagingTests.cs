using AnimalHaus.Contracts.Events;
using AnimalHaus.Shared.Messaging;
using AnimalHaus.Shared.Utils;

namespace AnimalHaus.Shared.Tests;

public sealed class MessagingTests
{
    [Fact]
    public void EnvelopeFactory_PopulatesRequiredMetadata()
    {
        var envelope = EnvelopeFactory.Create(
            TopicNames.Event("Pigpen", nameof(PigFed)),
            new PigFed("pig-001", 1, 3),
            "corr-123",
            "cause-456");

        Assert.Equal("pigpen.events.PigFed", envelope.Topic);
        Assert.Equal(nameof(PigFed), envelope.MessageType);
        Assert.Equal("corr-123", envelope.Metadata.CorrelationId);
        Assert.Equal("cause-456", envelope.Metadata.CausationId);
        Assert.Equal(TopicNames.SchemaVersion, envelope.Metadata.SchemaVersion);
        Assert.NotEqual(Guid.Empty, envelope.Metadata.MessageId);
        Assert.True(envelope.Metadata.TimestampUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void DeterministicRandomProvider_ProducesStableSequence()
    {
        var first = new DeterministicRandomProvider(17);
        var second = new DeterministicRandomProvider(17);

        var firstSequence = Enumerable.Range(0, 4).Select(_ => first.Next(1, 10)).ToArray();
        var secondSequence = Enumerable.Range(0, 4).Select(_ => second.Next(1, 10)).ToArray();

        Assert.Equal(firstSequence, secondSequence);
    }

    [Fact]
    public void JsonSerializer_RoundTripsContracts()
    {
        var original = new DispatchCompleted("feed", 2, "Pigpen", 4);
        var json = JsonMessageSerializer.Serialize(original);
        var copy = JsonMessageSerializer.Deserialize<DispatchCompleted>(json);

        Assert.Equal(original, copy);
    }
}
