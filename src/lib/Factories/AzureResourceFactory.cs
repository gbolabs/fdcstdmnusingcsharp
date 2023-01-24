using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using lib.Config;
using lib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace lib.Factories;

public class AzureResourceFactory
{
    protected ILogger? Logger { get; }
    private readonly IDescribeAzureResource _config;
    protected ArmClient Client { get; }

    protected AzureResourceFactory(ArmClientFactory clientFactory, IOptions<IDescribeAzureResource> azureResourceConfig,
        ILogger? logger)
    {
        Logger = logger;
        Client = clientFactory.Client;
        _config = azureResourceConfig.Value;
    }

    protected async Task<SubscriptionResource?> GetSubscriptionAsync(string? subscriptionId = null)
    {
        var id = subscriptionId.Or(_config.SubscriptionId);

        var subscriptionData = string.IsNullOrWhiteSpace(id)
            ? await Client.GetDefaultSubscriptionAsync().ConfigureAwait(false)
            : await Client.GetSubscriptionResource(new ResourceIdentifier(id)).GetAsync()
                .ConfigureAwait(false);

        return subscriptionData;
    }

    protected async Task<ResourceGroupResource?> GetResourceGroupAsync(string? resourceGroupName = null,
        string? subscriptionId = null)
    {
        var sub = await GetSubscriptionAsync(subscriptionId).ConfigureAwait(false);
        var rgName = resourceGroupName.Or(_config.ResourceGroupName).LogAndThrowWhenNull(
            $"Resource group name is not provided or default is not configured in config file with {nameof(_config.ResourceGroupName)}.",
            Logger);

        if (sub != null) return await sub.GetResourceGroups().GetAsync(rgName).ConfigureAwait(false);
        return default;
    }
}