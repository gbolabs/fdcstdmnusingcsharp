using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Cdn;
using Azure.ResourceManager.Cdn.Models;
using lib.Config;
using lib.Factories.Model;
using lib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace lib.Factories;

public class FrontDoorFactory : AzureResourceFactory
{
    private readonly FrontDoorFactoryConfig _factoryConfig;

    public FrontDoorFactory(ArmClientFactory clientFactory, IOptions<FrontDoorFactoryConfig> config,
        ILogger<DnsFactory>? logger) :
        base(clientFactory, config, logger)
    {
        _factoryConfig = config.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="customDomain">myhost.company.com</param>
    /// <param name="dnsZoneId">Azure Resource Id of company.com</param>
    /// <returns></returns>
    public async Task<FrontDoorCustomDomainResource> CreateCustomDomainAsync(string customDomain, string dnsZoneId)
    {
        var cdn = await GetCdnProfileAsync().ConfigureAwait(false);

        var response = await (await cdn.GetFrontDoorCustomDomains().CreateOrUpdateAsync(WaitUntil.Started,
            customDomain.Replace(".", string.Empty).Replace("_", string.Empty), new FrontDoorCustomDomainData
            {
                HostName = customDomain,
                DnsZoneId = new ResourceIdentifier(dnsZoneId)
            }).ConfigureAwait(false)).WaitForCompletionAsync().ConfigureAwait(false);

        return response.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="endPointName"></param>
    /// <param name="routeName">when null then use the First of the endpoints' routes</param>
    /// <returns></returns>
    public async Task<FrontDoorRouteResource> AssociateAsync(string customDomainId, string endPointName,
        string routeName = null, AssociationMode associationMode = AssociationMode.Append)
    {
        if (associationMode == AssociationMode.Undefined)
            throw new ArgumentException("AssociationMode must be defined");

        var route = await GetRouteAsync(endPointName, routeName).ConfigureAwait(false);

        var patch = new FrontDoorRoutePatch();

        if (associationMode == AssociationMode.Append)
            // Retrieves current list of custom domains and adds the new one.
            // Avoid duplicate entries.
            patch.CustomDomains.AddAll(
                route.Data.CustomDomains.Select(cd => cd.Id)
                    .Concat(new[] { new ResourceIdentifier(customDomainId) }).Distinct()
                    .Select(id => new FrontDoorActivatedResourceInfo
                    {
                        Id = new ResourceIdentifier(id)
                    }));
        else
            patch.CustomDomains.Add(new FrontDoorActivatedResourceInfo
            {
                Id = new ResourceIdentifier(customDomainId)
            });

        var updater = await route.UpdateAsync(WaitUntil.Completed, patch).ConfigureAwait(false);
        var result = await updater.WaitForCompletionAsync().ConfigureAwait(false);

        return result.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<FrontDoorEndpointResource> GetEndpointAsync(string name)
    {
        var cdn = await GetCdnProfileAsync().ConfigureAwait(false);
        return await cdn.GetFrontDoorEndpoints().GetAsync(name).ConfigureAwait(false);
    }

    private async Task<FrontDoorRouteResource> GetRouteAsync(string endpointName, string routeName = null)
    {
        var endpoint = await GetEndpointAsync(endpointName).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(routeName))
            return await endpoint.GetFrontDoorRoutes().GetAll().First().GetAsync().ConfigureAwait(false);

        return await endpoint.GetFrontDoorRouteAsync(routeName).ConfigureAwait(false);
    }

    private async Task<ProfileResource> GetCdnProfileAsync(string name = null)
    {
        var cdnName = string.IsNullOrWhiteSpace(name) ? _factoryConfig.FrontDoorName : name;
        if (string.IsNullOrWhiteSpace(cdnName))
        {
            var message =
                $"No FrontDoor name provided or configured in appsettings.json with {nameof(FrontDoorFactoryConfig)}.{nameof(FrontDoorFactoryConfig.FrontDoorName)}";
            Logger.LogCritical(message);
            throw new ApplicationException(message);
        }

        var rg = await GetResourceGroupAsync().ConfigureAwait(false);

        var cdn = await rg.GetProfileAsync(cdnName).ConfigureAwait(false);

        if (cdn == null || cdn.GetRawResponse().IsError)
        {
            var message = $"CDN profile {cdnName} not found";
            Logger.LogCritical(message);
            throw new Exception(message);
        }

        return cdn;
    }
}