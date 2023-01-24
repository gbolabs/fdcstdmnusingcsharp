using lib;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TestAzureSdk;

public class ConfiguredBasedTest : BaseTest
{
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddConfig<TestConfig>(Configuration);
    }

    protected override void SetupConfiguration(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.isago-labs.json")
            .AddEnvironmentVariables();
    }
    
    protected T GetConfig<T>()
        where T:class
    {
        return GetService<IOptions<T>>()?.Value;
    }

    protected IServiceProvider Build()
    {
        return base.Build(SetupServices);
    }
}