using System.Diagnostics;

namespace AnimalHaus.Integration.Tests;

public sealed class DistributedSimulationTests
{
    [Fact]
    public async Task Systems_RunAsSeparateProcesses_AndExchangeMessagesOverZeroMq()
    {
        var root = GetRepositoryRoot();
        var pigpen = StartProcess(Path.Combine(root, "src/systems/AnimalHaus.Pigpen/AnimalHaus.Pigpen.csproj"), new Dictionary<string, string>
        {
            ["ANIMALHAUS_TICK_INTERVAL_MS"] = "100",
            ["ANIMALHAUS_MAX_TICKS"] = "6",
            ["ANIMALHAUS_STARTUP_DELAY_MS"] = "500",
            ["ANIMALHAUS_PUB_ENDPOINT"] = "tcp://127.0.0.1:5811",
            ["ANIMALHAUS_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5812",
            ["ANIMALHAUS_PEER_BARN_PUB_ENDPOINT"] = "tcp://127.0.0.1:5821",
            ["ANIMALHAUS_PEER_BARN_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5822",
            ["ANIMALHAUS_PEER_TRACTOR_PUB_ENDPOINT"] = "tcp://127.0.0.1:5831",
            ["ANIMALHAUS_PEER_TRACTOR_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5832",
            ["ANIMALHAUS_PEER_MARKETPLACE_PUB_ENDPOINT"] = "tcp://127.0.0.1:5841",
            ["ANIMALHAUS_PEER_MARKETPLACE_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5842",
        });
        var barn = StartProcess(Path.Combine(root, "src/systems/AnimalHaus.Barn/AnimalHaus.Barn.csproj"), new Dictionary<string, string>
        {
            ["ANIMALHAUS_TICK_INTERVAL_MS"] = "100",
            ["ANIMALHAUS_MAX_TICKS"] = "6",
            ["ANIMALHAUS_STARTUP_DELAY_MS"] = "500",
            ["ANIMALHAUS_PUB_ENDPOINT"] = "tcp://127.0.0.1:5821",
            ["ANIMALHAUS_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5822",
            ["ANIMALHAUS_PEER_PIGPEN_PUB_ENDPOINT"] = "tcp://127.0.0.1:5811",
            ["ANIMALHAUS_PEER_PIGPEN_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5812",
            ["ANIMALHAUS_PEER_TRACTOR_PUB_ENDPOINT"] = "tcp://127.0.0.1:5831",
            ["ANIMALHAUS_PEER_TRACTOR_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5832",
            ["ANIMALHAUS_PEER_MARKETPLACE_PUB_ENDPOINT"] = "tcp://127.0.0.1:5841",
            ["ANIMALHAUS_PEER_MARKETPLACE_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5842",
        });
        var tractor = StartProcess(Path.Combine(root, "src/systems/AnimalHaus.Tractor/AnimalHaus.Tractor.csproj"), new Dictionary<string, string>
        {
            ["ANIMALHAUS_TICK_INTERVAL_MS"] = "100",
            ["ANIMALHAUS_MAX_TICKS"] = "6",
            ["ANIMALHAUS_STARTUP_DELAY_MS"] = "500",
            ["ANIMALHAUS_PUB_ENDPOINT"] = "tcp://127.0.0.1:5831",
            ["ANIMALHAUS_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5832",
            ["ANIMALHAUS_PEER_PIGPEN_PUB_ENDPOINT"] = "tcp://127.0.0.1:5811",
            ["ANIMALHAUS_PEER_PIGPEN_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5812",
            ["ANIMALHAUS_PEER_BARN_PUB_ENDPOINT"] = "tcp://127.0.0.1:5821",
            ["ANIMALHAUS_PEER_BARN_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5822",
            ["ANIMALHAUS_PEER_MARKETPLACE_PUB_ENDPOINT"] = "tcp://127.0.0.1:5841",
            ["ANIMALHAUS_PEER_MARKETPLACE_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5842",
        });
        var marketPlace = StartProcess(Path.Combine(root, "src/systems/AnimalHaus.MarketPlace/AnimalHaus.MarketPlace.csproj"), new Dictionary<string, string>
        {
            ["ANIMALHAUS_TICK_INTERVAL_MS"] = "100",
            ["ANIMALHAUS_MAX_TICKS"] = "6",
            ["ANIMALHAUS_STARTUP_DELAY_MS"] = "500",
            ["ANIMALHAUS_PUB_ENDPOINT"] = "tcp://127.0.0.1:5841",
            ["ANIMALHAUS_COMMAND_ENDPOINT"] = "tcp://127.0.0.1:5842",
        });

        var pigpenOutputTask = CaptureOutputAsync(pigpen);
        var barnOutputTask = CaptureOutputAsync(barn);
        var tractorOutputTask = CaptureOutputAsync(tractor);
        var marketPlaceOutputTask = CaptureOutputAsync(marketPlace);

        await Task.WhenAll(WaitForExitAsync(pigpen), WaitForExitAsync(barn), WaitForExitAsync(tractor), WaitForExitAsync(marketPlace));

        var pigpenOutput = await pigpenOutputTask;
        var barnOutput = await barnOutputTask;
        var tractorOutput = await tractorOutputTask;
        var marketPlaceOutput = await marketPlaceOutputTask;

        Assert.Contains("\"name\":\"DispatchCompleted\"", barnOutput);
        Assert.Contains("\"name\":\"InventoryChanged\"", barnOutput);
        Assert.Contains("\"name\":\"TractorDispatched\"", tractorOutput);
        Assert.Contains("\"name\":\"TaskCompleted\"", tractorOutput);
        Assert.Contains("\"name\":\"PigFed\"", pigpenOutput);
        Assert.Contains("\"name\":\"PigReadyForTransfer\"", pigpenOutput);
        Assert.Contains("\"name\":\"MarketPriceChanged\"", marketPlaceOutput);
        Assert.Contains("\"name\":\"MarketPriceChanged\"", pigpenOutput);
        Assert.Contains("\"name\":\"MarketPriceChanged\"", barnOutput);
        Assert.Contains("\"name\":\"MarketPriceChanged\"", tractorOutput);
    }

    private static string GetRepositoryRoot() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));

    private static Process StartProcess(string projectPath, IReadOnlyDictionary<string, string> environment)
    {
        var startInfo = new ProcessStartInfo("dotnet", $"run --no-build --project \"{projectPath}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(projectPath)!,
        };

        foreach (var item in environment)
        {
            startInfo.Environment[item.Key] = item.Value;
        }

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start {projectPath}");
        return process;
    }

    private static async Task<string> CaptureOutputAsync(Process process)
    {
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        var output = await stdout;
        var error = await stderr;
        return output + Environment.NewLine + error;
    }

    private static async Task WaitForExitAsync(Process process)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await process.WaitForExitAsync(cts.Token);
        Assert.Equal(0, process.ExitCode);
    }
}
