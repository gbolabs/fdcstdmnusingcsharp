using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TestAzureSdk;

public static class ServiceProviderExtensions
{
    public static T Get<T>(this IServiceProvider services)
    {
        return services.GetRequiredService<T>();
    }

    public static void AddConfig<T>(this IServiceCollection serviceCollection, IConfiguration configuration) 
        where T : class
    {
        serviceCollection.Configure<T>(configuration.GetSection(typeof(T).Name));
    }
}