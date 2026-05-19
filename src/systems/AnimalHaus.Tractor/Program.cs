using AnimalHaus.Tractor;
using AnimalHaus.Shared.Utils;
using NetMQ;

var config = SystemConfigurationLoader.Load(AppContext.BaseDirectory, "Tractor");
var options = SystemConfigurationLoader.LoadDomain<TractorOptions>(AppContext.BaseDirectory);
var host = new TractorHost(config, options);
await host.RunAsync(CancellationToken.None);
NetMQConfig.Cleanup(false);
