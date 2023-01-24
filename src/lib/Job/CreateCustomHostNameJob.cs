using Azure;
using Azure.ResourceManager.Cdn;
using Azure.ResourceManager.Dns;
using lib.Factories;
using Microsoft.Extensions.Logging;

namespace lib.Job;

public class CreateCustomHostNameJob
{
    private readonly ILogger<CreateCustomHostNameJob> _logger;
    private readonly DnsFactory _dnsFactory;
    private readonly FrontDoorFactory _frontDoorFactory;

    public CreateCustomHostNameJob(ILogger<CreateCustomHostNameJob> logger,
        DnsFactory dnsFactory, FrontDoorFactory frontDoorFactory)
    {
        _logger = logger;
        _dnsFactory = dnsFactory;
        _frontDoorFactory = frontDoorFactory;
    }

    public async Task<FrontDoorRouteResource> CreateCustomDomainAsync(string hostName, string targetEndpointName)
    {
        var zone = await _dnsFactory.GetDnsZoneAsync().ConfigureAwait(false);
        return await CreateCustomDomainInDnsZoneAsync(hostName, targetEndpointName, zone).ConfigureAwait(false);
    }

    public async Task<FrontDoorRouteResource> CreateCustomDomainWithSubZoneAsync(string hostName, string? subZoneName,
        string targetEndpointName)
    {
        
        var subZone = await _dnsFactory.GetOrCreateDnsSubZoneAsync(subZoneName).ConfigureAwait(false);
        return await CreateCustomDomainInDnsZoneAsync(hostName, targetEndpointName, subZone).ConfigureAwait(false);
    }

    private async Task<FrontDoorRouteResource> CreateCustomDomainInDnsZoneAsync(string hostName, string targetEndpointName,
        DnsZoneResource dnsZone)
    {
        var newFullHostName = $"{hostName}.{dnsZone.Data.Name}";

        var domain = await _frontDoorFactory.CreateCustomDomainAsync(newFullHostName, dnsZone.Id).ConfigureAwait(false);
        var endpoint = await _frontDoorFactory.GetEndpointAsync(targetEndpointName).ConfigureAwait(false);

        var txt = await _dnsFactory.CreateTxtRecord("_dnsauth." + hostName,
            domain.Data.ValidationProperties.ValidationToken, dnsZone: dnsZone.Data.Name).ConfigureAwait(false);

        var cname = await _dnsFactory.CreateCnameRecord(hostName,
            endpoint.Data.HostName, dnsZone: dnsZone.Data.Name).ConfigureAwait(false);

        try
        {
            var associated =
                await _frontDoorFactory.AssociateAsync(domain.Id, endpoint.Data.Name).ConfigureAwait(false);

            return associated;
        }
        catch (RequestFailedException requestFailedException)
        {
            _logger.LogError(requestFailedException, "Failed to associate domain {domain} with endpoint {endpoint}",
                domain.Data.Name, endpoint.Data.Name);
            throw;
        }
    }
}