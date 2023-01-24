using lib.Config;
using lib.Factories;
using lib.Job;
using lib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace lib;

public static class Registrar
{
    public static IServiceCollection AddAzureSdk(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddScopedWithConfig<ArmClientFactory, ArmClientFactoryConfig>(configuration)
            .AddScopedWithConfig<DnsFactory, DnsFactoryConfig>(configuration)
            .AddScopedWithConfig<FrontDoorFactory, FrontDoorFactoryConfig>(configuration)
            .AddTransient<CreateCustomHostNameJob>();
        return services;
    }

    private static IServiceCollection AddScopedWithConfig<TService, TConfig>(this IServiceCollection services,
        IConfiguration configuration) where TConfig : class where TService : class
    {
        services.AddConfig<TConfig>(configuration);
        services.AddScoped<TService>();
        return services;
    }

    private static IServiceCollection AddTransientWithConfig<TService, TConfig>(this IServiceCollection services,
        IConfiguration configuration) where TConfig : class where TService : class
    {
        services.AddConfig<TConfig>(configuration);
        services.AddTransient<TService>();
        return services;
    }

    private static IServiceCollection AddConfig<TConfig>(this IServiceCollection serviceCollection,
        IConfiguration configuration) where TConfig : class
    {
        serviceCollection.Configure<TConfig>(configuration.GetSection(typeof(TConfig).Name));
        return serviceCollection;
    }
}