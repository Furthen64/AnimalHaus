using AnimalHaus.Contracts.Commands;
using AnimalHaus.Contracts.Events;
using AnimalHaus.Shared.Core;
using AnimalHaus.Shared.Messaging;
using AnimalHaus.Shared.Utils;
using AnimalHaus.Pigpen.Modules;

public sealed class PigpenHost
{
    private readonly SystemConfiguration config;
    private readonly DeterministicRandomProvider randomProvider;
    private readonly PigLifecycleModule lifecycleModule = new();
    private readonly FeedingModule feedingModule = new();
    private readonly HealthModule healthModule = new();
    private readonly PenEnvironmentModule environmentModule = new();
    private readonly string pigId = "pig-001";
    private bool dispatchRequested;
    private bool dispatchCompleted;
    private bool readyPublished;
    private int deliveredFeed;

    public PigpenHost(SystemConfiguration config)
    {
        this.config = config;
        randomProvider = new DeterministicRandomProvider(config.Simulation.Seed);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var publisher = new NetMqPublisher(config.Messaging.PubEndpoint);
        using var subscriber = new NetMqSubscriber(
            config.Messaging.Peers.Values.Select(static peer => peer.PubEndpoint),
            ["barn.events.", "tractor.events.", "marketplace.events."]);
        using var commandServer = new NetMqCommandServer(config.Messaging.CommandEndpoint);
        var commandClient = new NetMqCommandClient();

        StructuredLog.Write(config.SystemName, "lifecycle", "started", data: new { config.Messaging.PubEndpoint, config.Messaging.CommandEndpoint });
        await Task.Delay(config.Simulation.StartupDelayMs, cancellationToken);

        for (var tick = 1; tick <= config.Simulation.MaxTicks; tick++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var context = new TickContext(tick, DateTime.UtcNow);

            DrainCommands(commandServer);
            DrainEvents(subscriber, tick);

            if (!dispatchRequested && tick >= 2 && feedingModule.NeedsFeed)
            {
                try
                {
                    var response = commandClient.Send<RequestDispatch, CommandAccepted>(
                        config.Messaging.Peers["Barn"].CommandEndpoint,
                        TopicNames.Command("Barn", nameof(RequestDispatch)),
                        new RequestDispatch("feed", 2, config.SystemName),
                        CorrelationIds.New());

                    dispatchRequested = response.Accepted;
                    StructuredLog.Write(config.SystemName, "command", nameof(RequestDispatch), tick, data: response);
                }
                catch (TimeoutException ex)
                {
                    StructuredLog.Write(config.SystemName, "warning", "RequestDispatchTimeout", tick, data: new
                    {
                        endpoint = config.Messaging.Peers["Barn"].CommandEndpoint,
                        ex.Message,
                    });
                }
            }

            var wasFed = feedingModule.AdvanceTick();
            environmentModule.AdvanceTick(randomProvider);
            lifecycleModule.AdvanceTick(wasFed);
            var healthChanged = healthModule.Update(feedingModule.HungerScore, environmentModule.CleanlinessScore);

            if (wasFed)
            {
                PublishEvent(publisher, TopicNames.Event(config.SystemName, nameof(PigFed)), new PigFed(pigId, 1, tick), tick);
            }

            if (healthChanged)
            {
                PublishEvent(publisher, TopicNames.Event(config.SystemName, nameof(PigHealthChanged)), new PigHealthChanged(pigId, healthModule.HealthScore, tick), tick);
            }

            if (!readyPublished && lifecycleModule.IsReadyForTransfer)
            {
                readyPublished = true;
                PublishEvent(publisher, TopicNames.Event(config.SystemName, nameof(PigReadyForTransfer)), new PigReadyForTransfer(pigId, lifecycleModule.Weight, tick), tick);
            }

            StructuredLog.Write(config.SystemName, "tick", "state", tick, data: new
            {
                feedingModule.FeedStock,
                feedingModule.HungerScore,
                healthModule.HealthScore,
                lifecycleModule.Weight,
                environmentModule.CleanlinessScore,
                dispatchCompleted,
            });

            await Task.Delay(config.Simulation.TickIntervalMs, cancellationToken);
        }

        StructuredLog.Write(config.SystemName, "lifecycle", "stopped");
    }

    private void DrainEvents(NetMqSubscriber subscriber, int tick)
    {
        while (subscriber.TryReceive(out var envelope))
        {
            switch (envelope.MessageType)
            {
                case nameof(DispatchCompleted):
                    var dispatchCompletedEvent = JsonMessageSerializer.Deserialize<DispatchCompleted>(envelope.PayloadJson);
                    if (string.Equals(dispatchCompletedEvent.DestinationSystem, config.SystemName, StringComparison.OrdinalIgnoreCase))
                    {
                        dispatchCompleted = true;
                        deliveredFeed = dispatchCompletedEvent.Quantity;
                        StructuredLog.Write(config.SystemName, "event", nameof(DispatchCompleted), tick, envelope.Metadata.CorrelationId, dispatchCompletedEvent);
                    }
                    break;

                case nameof(TaskCompleted):
                    var taskCompleted = JsonMessageSerializer.Deserialize<TaskCompleted>(envelope.PayloadJson);
                    if (dispatchCompleted && string.Equals(taskCompleted.DestinationSystem, config.SystemName, StringComparison.OrdinalIgnoreCase))
                    {
                        feedingModule.ReceiveFeed(deliveredFeed);
                        dispatchCompleted = false;
                        deliveredFeed = 0;
                        StructuredLog.Write(config.SystemName, "event", nameof(TaskCompleted), tick, envelope.Metadata.CorrelationId, taskCompleted);
                    }
                    break;

                case nameof(MarketPriceChanged):
                    var marketPriceChanged = JsonMessageSerializer.Deserialize<MarketPriceChanged>(envelope.PayloadJson);
                    StructuredLog.Write(config.SystemName, "event", nameof(MarketPriceChanged), tick, envelope.Metadata.CorrelationId, marketPriceChanged);
                    break;
            }
        }
    }

    private void DrainCommands(NetMqCommandServer commandServer)
    {
        while (commandServer.TryReceive(out var envelope))
        {
            switch (envelope.MessageType)
            {
                case nameof(DeliverFeed):
                    var deliverFeed = JsonMessageSerializer.Deserialize<DeliverFeed>(envelope.PayloadJson);
                    feedingModule.ReceiveFeed(deliverFeed.Quantity);
                    commandServer.Reply(TopicNames.Command(config.SystemName, nameof(DeliverFeed)), new CommandAccepted(true, "Feed delivered"), envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
                    break;

                case nameof(TransferPig):
                    commandServer.Reply(TopicNames.Command(config.SystemName, nameof(TransferPig)), new CommandAccepted(true, "Pig ready for transfer"), envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
                    break;
            }
        }
    }

    private void PublishEvent<T>(NetMqPublisher publisher, string topic, T payload, int tick)
    {
        var envelope = EnvelopeFactory.Create(topic, payload, CorrelationIds.New());
        publisher.Publish(envelope);
        StructuredLog.Write(config.SystemName, "event", envelope.MessageType, tick, envelope.Metadata.CorrelationId, payload);
    }
}
