using System.Text.Json;
using System.Text.Json.Nodes;
using AnimalHaus.Shared.Core;

namespace AnimalHaus.Shared.Utils;

public static class SystemConfigurationLoader
{
    public static TDomain LoadDomain<TDomain>(string basePath) where TDomain : new()
    {
        var appSettingsPath = Path.Combine(basePath, "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
            return new TDomain();
        }

        var root = JsonNode.Parse(File.ReadAllText(appSettingsPath));
        var domainNode = root?["domain"];
        if (domainNode is null)
        {
            return new TDomain();
        }

        return JsonSerializer.Deserialize<TDomain>(domainNode.ToJsonString(), JsonMessageSerializer.DefaultOptions)
            ?? new TDomain();
    }

    public static SystemConfiguration Load(string basePath, string defaultSystemName)
    {
        var config = new SystemConfiguration { SystemName = defaultSystemName };
        var appSettingsPath = Path.Combine(basePath, "appsettings.json");
        if (File.Exists(appSettingsPath))
        {
            config = JsonSerializer.Deserialize<SystemConfiguration>(File.ReadAllText(appSettingsPath), JsonMessageSerializer.DefaultOptions)
                ?? config;

            if (string.IsNullOrWhiteSpace(config.SystemName))
            {
                config.SystemName = defaultSystemName;
            }
        }

        ApplyEnvironmentOverrides(config);
        return config;
    }

    private static void ApplyEnvironmentOverrides(SystemConfiguration config)
    {
        config.SystemName = Get("ANIMALHAUS_SYSTEM_NAME", config.SystemName);
        config.Simulation.TickIntervalMs = GetInt("ANIMALHAUS_TICK_INTERVAL_MS", config.Simulation.TickIntervalMs);
        config.Simulation.MaxTicks = GetInt("ANIMALHAUS_MAX_TICKS", config.Simulation.MaxTicks);
        config.Simulation.StartupDelayMs = GetInt("ANIMALHAUS_STARTUP_DELAY_MS", config.Simulation.StartupDelayMs);
        config.Simulation.Seed = GetInt("ANIMALHAUS_SEED", config.Simulation.Seed);
        config.Messaging.PubEndpoint = Get("ANIMALHAUS_PUB_ENDPOINT", config.Messaging.PubEndpoint);
        config.Messaging.CommandEndpoint = Get("ANIMALHAUS_COMMAND_ENDPOINT", config.Messaging.CommandEndpoint);

        foreach (var peer in config.Messaging.Peers)
        {
            var peerName = peer.Key.ToUpperInvariant();
            peer.Value.PubEndpoint = Get($"ANIMALHAUS_PEER_{peerName}_PUB_ENDPOINT", peer.Value.PubEndpoint);
            peer.Value.CommandEndpoint = Get($"ANIMALHAUS_PEER_{peerName}_COMMAND_ENDPOINT", peer.Value.CommandEndpoint);
        }
    }

    private static string Get(string key, string fallback) => Environment.GetEnvironmentVariable(key) is { Length: > 0 } value ? value : fallback;

    private static int GetInt(string key, int fallback) => int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;
}
