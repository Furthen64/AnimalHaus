namespace AnimalHaus.Shared.Core;

public sealed class SimulationOptions
{
    public int TickIntervalMs { get; set; } = 250;

    public int MaxTicks { get; set; } = 8;

    public int StartupDelayMs { get; set; } = 750;

    public int Seed { get; set; } = 12345;
}

public sealed class PeerConfiguration
{
    public string PubEndpoint { get; set; } = string.Empty;

    public string CommandEndpoint { get; set; } = string.Empty;
}

public sealed class MessagingOptions
{
    public string PubEndpoint { get; set; } = string.Empty;

    public string CommandEndpoint { get; set; } = string.Empty;

    public Dictionary<string, PeerConfiguration> Peers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class SystemConfiguration
{
    public string SystemName { get; set; } = string.Empty;

    public SimulationOptions Simulation { get; set; } = new();

    public MessagingOptions Messaging { get; set; } = new();
}
