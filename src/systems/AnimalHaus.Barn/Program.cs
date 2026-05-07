using AnimalHaus.Shared.Utils;
using NetMQ;

var config = SystemConfigurationLoader.Load(AppContext.BaseDirectory, "Barn");
var host = new BarnHost(config);
await host.RunAsync(CancellationToken.None);
NetMQConfig.Cleanup(false);
