using AnimalHaus.Contracts.Commands;
using AnimalHaus.Contracts.Events;
using AnimalHaus.Shared.Core;
using AnimalHaus.Shared.Messaging;
using AnimalHaus.Shared.Utils;
using AnimalHaus.Tractor.Modules;

public sealed class TractorHost
{
    private readonly SystemConfiguration config;
    private readonly RoutePlanningModule routePlanningModule = new();
    private readonly FuelModule fuelModule = new();
    private readonly HaulingModule haulingModule = new();
    private readonly MaintenanceModule maintenanceModule = new();
    private readonly string tractorId = "tractor-01";

    public TractorHost(SystemConfiguration config)
    {
        this.config = config;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var publisher = new NetMqPublisher(config.Messaging.PubEndpoint);
        using var subscriber = new NetMqSubscriber(
            config.Messaging.Peers.Values.Select(static peer => peer.PubEndpoint),
            ["marketplace.events."]);
        using var commandServer = new NetMqCommandServer(config.Messaging.CommandEndpoint);

        StructuredLog.Write(config.SystemName, "lifecycle", "started", data: new { fuelModule.FuelLevel });
        await Task.Delay(config.Simulation.StartupDelayMs, cancellationToken);

        for (var tick = 1; tick <= config.Simulation.MaxTicks; tick++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DrainEvents(subscriber, tick);
            DrainCommands(commandServer, publisher, tick);

            if (haulingModule.TryCompleteTick(out var completedTask))
            {
                fuelModule.Consume(12);
                maintenanceModule.RecordHaul();
                PublishEvent(publisher, TopicNames.Event(config.SystemName, nameof(TaskCompleted)), new TaskCompleted(tractorId, completedTask.TaskName, completedTask.DestinationSystem, tick), tick, completedTask.CorrelationId, completedTask.CausationId);

                if (fuelModule.IsLow)
                {
                    PublishEvent(publisher, TopicNames.Event(config.SystemName, nameof(FuelLow)), new FuelLow(tractorId, fuelModule.FuelLevel, tick), tick, completedTask.CorrelationId, completedTask.CausationId);
                }
            }

            StructuredLog.Write(config.SystemName, "tick", "state", tick, data: new
            {
                fuelModule.FuelLevel,
                maintenanceModule.WearScore,
                haulingModule.HasPendingTask,
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
            }
        }
    }

    private void DrainCommands(NetMqCommandServer commandServer, NetMqPublisher publisher, int tick)
    {
        while (commandServer.TryReceive(out var envelope))
        {
            switch (envelope.MessageType)
            {
                case nameof(AssignTask):
                    var assignTask = JsonMessageSerializer.Deserialize<AssignTask>(envelope.PayloadJson);
                    var route = routePlanningModule.Plan(assignTask.DestinationSystem);
                    haulingModule.Assign(assignTask.TaskName, assignTask.DestinationSystem, envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"), route);
                    PublishEvent(publisher, TopicNames.Event(config.SystemName, nameof(TractorDispatched)), new TractorDispatched(tractorId, assignTask.TaskName, assignTask.DestinationSystem, tick), tick, envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
                    commandServer.Reply(TopicNames.Command(config.SystemName, nameof(AssignTask)), new CommandAccepted(true, $"Assigned via {route}"), envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
                    break;

                case nameof(RefuelTractor):
                    var refuel = JsonMessageSerializer.Deserialize<RefuelTractor>(envelope.PayloadJson);
                    fuelModule.Refuel(refuel.Quantity);
                    commandServer.Reply(TopicNames.Command(config.SystemName, nameof(RefuelTractor)), new CommandAccepted(true, "Refueled"), envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
                    break;

                case nameof(ScheduleMaintenance):
                    maintenanceModule.Schedule();
                    commandServer.Reply(TopicNames.Command(config.SystemName, nameof(ScheduleMaintenance)), new CommandAccepted(true, "Maintenance scheduled"), envelope.Metadata.CorrelationId, envelope.Metadata.MessageId.ToString("N"));
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
