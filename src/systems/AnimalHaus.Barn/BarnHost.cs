using AnimalHaus.Barn.Modules;
using AnimalHaus.Contracts.Commands;
using AnimalHaus.Contracts.Events;
using AnimalHaus.Shared.Core;
using AnimalHaus.Shared.Messaging;
using AnimalHaus.Shared.Utils;

public sealed class BarnHost
{
    private readonly SystemConfiguration config;
    private readonly DeterministicRandomProvider randomProvider;
    private readonly InventoryModule inventoryModule = new();
    private readonly StorageCapacityModule storageCapacityModule = new();
    private readonly DispatchModule dispatchModule = new();
    private readonly QualityControlModule qualityControlModule = new();
    private bool resourceLowPublished;

    public BarnHost(SystemConfiguration config)
    {
        this.config = config;
        randomProvider = new DeterministicRandomProvider(config.Simulation.Seed);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var publisher = new NetMqPublisher(config.Messaging.PubEndpoint);
        using var subscriber = new NetMqSubscriber(
            config.Messaging.Peers.Values.Select(static peer => peer.PubEndpoint),
            ["pigpen.events.", "tractor.events.", "marketplace.events."]);
        using var commandServer = new NetMqCommandServer(config.Messaging.CommandEndpoint);
        var commandClient = new NetMqCommandClient();

        StructuredLog.Write(config.SystemName, "lifecycle", "started", data: new { inventoryModule.FeedUnits });
        await Task.Delay(config.Simulation.StartupDelayMs, cancellationToken);

        for (var tick = 1; tick <= config.Simulation.MaxTicks; tick++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DrainEvents(subscriber, tick);
            DrainCommands(commandServer, commandClient, publisher, tick);
            qualityControlModule.AdvanceTick(randomProvider);

            if (!resourceLowPublished && inventoryModule.FeedUnits <= 2)
            {
                resourceLowPublished = true;
                PublishEvent(publisher, TopicNames.Event(config.SystemName, nameof(ResourceLow)), new ResourceLow("feed", inventoryModule.FeedUnits, tick), tick);
            }

            StructuredLog.Write(config.SystemName, "tick", "state", tick, data: new
            {
                inventoryModule.FeedUnits,
                storageCapacityModule.Capacity,
                qualityControlModule.FreshnessScore,
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
                case nameof(MarketPriceChanged):
                    var marketPriceChanged = JsonMessageSerializer.Deserialize<MarketPriceChanged>(envelope.PayloadJson);
                    StructuredLog.Write(config.SystemName, "event", nameof(MarketPriceChanged), tick, envelope.Metadata.CorrelationId, marketPriceChanged);
                    break;

                default:
                    StructuredLog.Write(config.SystemName, "event", envelope.MessageType, tick, envelope.Metadata.CorrelationId);
                    break;
            }
        }
    }

    private void DrainCommands(NetMqCommandServer commandServer, NetMqCommandClient commandClient, NetMqPublisher publisher, int tick)
    {
        while (commandServer.TryReceive(out var envelope))
        {
            switch (envelope.MessageType)
            {
                case nameof(RequestDispatch):
                    var requestDispatch = JsonMessageSerializer.Deserialize<RequestDispatch>(envelope.PayloadJson);
                    if (!storageCapacityModule.CanAllocate(requestDispatch.Quantity) || !inventoryModule.TryAllocateFeed(requestDispatch.Quantity))
                    {
                        commandServer.Reply(TopicNames.Command(config.SystemName, nameof(RequestDispatch)), new CommandAccepted(false, "Insufficient inventory"), envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
                        break;
                    }

                    PublishEvent(publisher, TopicNames.Event(config.SystemName, nameof(InventoryChanged)), new InventoryChanged("feed", inventoryModule.FeedUnits, tick), tick, envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));

                    var assignment = commandClient.Send<AssignTask, CommandAccepted>(
                        config.Messaging.Peers["Tractor"].CommandEndpoint,
                        TopicNames.Command("Tractor", nameof(AssignTask)),
                        new AssignTask(dispatchModule.CreateTaskName(tick), requestDispatch.DestinationSystem, requestDispatch.ResourceName, requestDispatch.Quantity),
                        envelope.Metadata.CorrelationId,
                        envelope.Metadata.MessageId.ToString("N"));

                    PublishEvent(publisher, TopicNames.Event(config.SystemName, nameof(DispatchCompleted)), new DispatchCompleted(requestDispatch.ResourceName, requestDispatch.Quantity, requestDispatch.DestinationSystem, tick), tick, envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
                    commandServer.Reply(TopicNames.Command(config.SystemName, nameof(RequestDispatch)), assignment, envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
                    break;

                case nameof(ReserveResource):
                    commandServer.Reply(TopicNames.Command(config.SystemName, nameof(ReserveResource)), new CommandAccepted(true, "Reserved"), envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
                    break;

                case nameof(ReleaseResource):
                    commandServer.Reply(TopicNames.Command(config.SystemName, nameof(ReleaseResource)), new CommandAccepted(true, "Released"), envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
                    break;
            }
        }
    }

    private void PublishEvent<T>(NetMqPublisher publisher, string topic, T payload, int tick, string? correlationId = null, string? causationId = null)
    {
        var envelope = EnvelopeFactory.Create(topic, payload, correlationId ?? CorrelationIds.New(), causationId);
        publisher.Publish(envelope);
        StructuredLog.Write(config.SystemName, "event", envelope.MessageType, tick, envelope.Metadata.CorrelationId, payload);
    }
}
