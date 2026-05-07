using AnimalHaus.Shared.Utils;
using NetMQ;

var config = SystemConfigurationLoader.Load(AppContext.BaseDirectory, "Tractor");
var host = new TractorHost(config);
await host.RunAsync(CancellationToken.None);
NetMQConfig.Cleanup(false);
