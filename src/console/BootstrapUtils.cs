using Azure.ResourceManager.Dns;
using lib.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace console;

public static class BootstrapUtils
{
    /// <summary>
    /// Default is appsettings.json and EnvironmentVariables being added to the configuration
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    internal static IConfiguration BuildConfiguration(Action<IConfigurationBuilder>? configure = null)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();

        configure?.Invoke(configuration);

        return configuration.Build();
    }

    internal static ILogger<T> GetLogger<T>(this IServiceProvider serviceProvider)
    {
        return serviceProvider.Bootstrap<ILogger<T>>();
    }

    internal static TService Bootstrap<TService>(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<TService>();
    }

    internal static async Task<TReturn> RunWithService<TService, TReturn>(this IServiceProvider provider,
        Func<TService, Task<TReturn>> func)
    {
        var service = provider.Bootstrap<TService>();
        try
        {
            return await func(service).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            provider.GetLogger<TService>().LogError(e, "Error running {Service}", typeof(TService).Name);
            throw;
        }
    }

    internal static async Task RunAsScoped<TService>(this ServiceProvider serviceProvider, Action<TService> action)
    {
        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        action(service);
    }

    internal static async Task<TReturn?> InvokeAsScopedAsync<TService, TReturn>(this IServiceProvider serviceProvider,
        Func<TService, Task<TReturn?>> func)
    {
        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return await func(service).ConfigureAwait(false);
    }
}