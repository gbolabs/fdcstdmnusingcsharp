using Azure.ResourceManager.Dns;
using console;
using lib;
using lib.Factories;
using lib.Job;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configuration = BootstrapUtils.BuildConfiguration(configure =>
{
    configure.AddJsonFile("appsettings.isago-labs.json");
});
var services = new ServiceCollection()
    .AddAzureSdk(configuration)
    .AddLogging(configure => { configure.AddConsole(); });
var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();
var dnsFactory = provider.GetRequiredService<DnsFactory>();


var zone = await dnsFactory.GetDnsZoneAsync("appzone.labs.isago.ch");
logger.LogInformation(zone.Data.Name);

provider.RunAsScoped<DnsFactory>(async factory =>
    await factory.GetDnsZoneAsync("lab01.labs.isago.ch").ConfigureAwait(false));
var result1 = provider.RunWithService<DnsFactory, DnsZoneResource?>(async factory =>
    await factory.GetDnsZoneAsync("lab01.labs.isago.ch").ConfigureAwait(false));
var result = await dnsFactory.GetDnsZoneAsync("test01.labs.isago.ch").ConfigureAwait(false);
var parentZone = await dnsFactory.GetDnsZoneAsync("labs.isago.ch").ConfigureAwait(false);
logger.LogInformation(parentZone.Data.Location);

var job = provider.GetRequiredService<CreateCustomHostNameJob>();

// await job.CreateCustomDomainWithSubZoneAsync("test01", "appzone2", "fd-csharparm-lab-01").ConfigureAwait(false);
await job.CreateCustomDomainWithSubZoneAsync("test05", "appzone2", "statapp2").ConfigureAwait(false);

logger.LogInformation("Done");