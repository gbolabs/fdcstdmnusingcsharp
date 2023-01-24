using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TestAzureSdk;

public abstract class BaseTest
{
    private IServiceProvider _services;

    protected BaseTest()
    {
        var configurationBuilder = new ConfigurationBuilder();
        SetupConfiguration(configurationBuilder);
        Configuration = configurationBuilder.Build();

        SetupServices(ServiceCollection);
        SetupEnvironmentVariables(Configuration);
    }

    protected virtual void SetupEnvironmentVariables(IConfiguration configuration)
    {
        
    }

    protected abstract void SetupServices(IServiceCollection serviceCollection);

    protected abstract void SetupConfiguration(IConfigurationBuilder configurationBuilder);

    private ServiceCollection ServiceCollection { get; } = new();

    protected IConfiguration Configuration { get; }

    protected IServiceProvider Services
    {
        get
        {
            if (_services == null)
                Build();

            return _services;
        }
    }
    
    protected IServiceProvider Build(Action<IServiceCollection> builder)
    {
        var serviceCollection = new ServiceCollection();
        
        builder.Invoke(serviceCollection);

        return serviceCollection.BuildServiceProvider();
    }

    protected T GetService<T>() => Services.GetRequiredService<T>();

    private void Build()
    {
        _services = ServiceCollection.BuildServiceProvider();
    }
}