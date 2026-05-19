using AnimalHaus.Pigpen;
using AnimalHaus.Shared.Utils;
using NetMQ;

var config = SystemConfigurationLoader.Load(AppContext.BaseDirectory, "Pigpen");
var options = SystemConfigurationLoader.LoadDomain<PigpenOptions>(AppContext.BaseDirectory);
var host = new PigpenHost(config, options);
await host.RunAsync(CancellationToken.None);
NetMQConfig.Cleanup(false);
