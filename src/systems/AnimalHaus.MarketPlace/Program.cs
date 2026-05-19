using AnimalHaus.MarketPlace;
using AnimalHaus.Shared.Utils;
using NetMQ;

var config = SystemConfigurationLoader.Load(AppContext.BaseDirectory, "MarketPlace");
var options = SystemConfigurationLoader.LoadDomain<MarketPlaceOptions>(AppContext.BaseDirectory);
var host = new MarketPlaceHost(config, options);
await host.RunAsync(CancellationToken.None);
NetMQConfig.Cleanup(false);
