using AnimalHaus.Contracts.Events;
using AnimalHaus.MarketPlace;
using AnimalHaus.Shared.Core;
using AnimalHaus.Shared.Messaging;
using AnimalHaus.Shared.Utils;

public sealed class MarketPlaceHost
{
    private readonly SystemConfiguration config;
    private readonly MarketPlaceOptions options;
    private readonly DeterministicRandomProvider randomProvider;
    private readonly Dictionary<string, decimal> prices;

    public MarketPlaceHost(SystemConfiguration config, MarketPlaceOptions options)
    {
        this.config = config;
        this.options = options;
        randomProvider = new DeterministicRandomProvider(config.Simulation.Seed);
        prices = new Dictionary<string, decimal>(options.Commodities, StringComparer.OrdinalIgnoreCase);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var publisher = new NetMqPublisher(config.Messaging.PubEndpoint);

        StructuredLog.Write(config.SystemName, "lifecycle", "started", data: new { config.Messaging.PubEndpoint });
        await Task.Delay(config.Simulation.StartupDelayMs, cancellationToken);

        for (var tick = 1; tick <= config.Simulation.MaxTicks; tick++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (tick % options.PriceUpdateEveryNTicks == 0)
            {
                foreach (var commodity in prices.Keys.ToList())
                {
                    prices[commodity] = NextPrice(prices[commodity]);
                    PublishEvent(publisher, new MarketPriceChanged(commodity, prices[commodity], options.Currency, tick), tick);
                }
            }

            StructuredLog.Write(config.SystemName, "tick", "state", tick, data: new { Prices = prices });

            await Task.Delay(config.Simulation.TickIntervalMs, cancellationToken);
        }

        StructuredLog.Write(config.SystemName, "lifecycle", "stopped");
    }

    private decimal NextPrice(decimal current)
    {
        var delta = randomProvider.Next(options.PriceDeltaMin, options.PriceDeltaMax + 1) / 100m;
        var next = current + delta;
        return decimal.Round(Math.Max(options.MinPrice, next), 2, MidpointRounding.AwayFromZero);
    }

    private void PublishEvent(NetMqPublisher publisher, MarketPriceChanged payload, int tick)
    {
        var envelope = EnvelopeFactory.Create(TopicNames.Event(config.SystemName, nameof(MarketPriceChanged)), payload, CorrelationIds.New());
        publisher.Publish(envelope);
        StructuredLog.Write(config.SystemName, "event", envelope.MessageType, tick, envelope.Metadata.CorrelationId, payload);
    }
}
