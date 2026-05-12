using AnimalHaus.Contracts.Events;
using AnimalHaus.Shared.Core;
using AnimalHaus.Shared.Messaging;
using AnimalHaus.Shared.Utils;

public sealed class MarketPlaceHost
{
    private readonly SystemConfiguration config;
    private readonly DeterministicRandomProvider randomProvider;
    private decimal eggsPrice = 3.10m;
    private decimal milkPrice = 2.45m;

    public MarketPlaceHost(SystemConfiguration config)
    {
        this.config = config;
        randomProvider = new DeterministicRandomProvider(config.Simulation.Seed);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var publisher = new NetMqPublisher(config.Messaging.PubEndpoint);

        StructuredLog.Write(config.SystemName, "lifecycle", "started", data: new { config.Messaging.PubEndpoint });
        await Task.Delay(config.Simulation.StartupDelayMs, cancellationToken);

        for (var tick = 1; tick <= config.Simulation.MaxTicks; tick++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            eggsPrice = NextPrice(eggsPrice);
            milkPrice = NextPrice(milkPrice);

            PublishEvent(publisher, new MarketPriceChanged("eggs", eggsPrice, "USD", tick), tick);
            PublishEvent(publisher, new MarketPriceChanged("milk", milkPrice, "USD", tick), tick);

            StructuredLog.Write(config.SystemName, "tick", "state", tick, data: new
            {
                Eggs = eggsPrice,
                Milk = milkPrice,
            });

            await Task.Delay(config.Simulation.TickIntervalMs, cancellationToken);
        }

        StructuredLog.Write(config.SystemName, "lifecycle", "stopped");
    }

    private decimal NextPrice(decimal current)
    {
        var delta = randomProvider.Next(-6, 7) / 100m;
        var next = current + delta;
        return decimal.Round(Math.Max(0.50m, next), 2, MidpointRounding.AwayFromZero);
    }

    private void PublishEvent(NetMqPublisher publisher, MarketPriceChanged payload, int tick)
    {
        var envelope = EnvelopeFactory.Create(TopicNames.Event(config.SystemName, nameof(MarketPriceChanged)), payload, CorrelationIds.New());
        publisher.Publish(envelope);
        StructuredLog.Write(config.SystemName, "event", envelope.MessageType, tick, envelope.Metadata.CorrelationId, payload);
    }
}
