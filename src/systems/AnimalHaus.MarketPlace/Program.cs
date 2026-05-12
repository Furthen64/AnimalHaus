using AnimalHaus.Shared.Utils;
using NetMQ;

var config = SystemConfigurationLoader.Load(AppContext.BaseDirectory, "MarketPlace");
var host = new MarketPlaceHost(config);
await host.RunAsync(CancellationToken.None);
NetMQConfig.Cleanup(false);
