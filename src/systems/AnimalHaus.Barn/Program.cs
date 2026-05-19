using AnimalHaus.Barn;
using AnimalHaus.Shared.Utils;
using NetMQ;

var config = SystemConfigurationLoader.Load(AppContext.BaseDirectory, "Barn");
var options = SystemConfigurationLoader.LoadDomain<BarnOptions>(AppContext.BaseDirectory);
var host = new BarnHost(config, options);
await host.RunAsync(CancellationToken.None);
NetMQConfig.Cleanup(false);
