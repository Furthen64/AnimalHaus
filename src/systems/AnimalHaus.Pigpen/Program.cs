using AnimalHaus.Shared.Utils;
using NetMQ;

var config = SystemConfigurationLoader.Load(AppContext.BaseDirectory, "Pigpen");
var host = new PigpenHost(config);
await host.RunAsync(CancellationToken.None);
NetMQConfig.Cleanup(false);
