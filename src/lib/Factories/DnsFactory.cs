using Azure;
using Azure.ResourceManager.Dns;
using Azure.ResourceManager.Dns.Models;
using lib.Config;
using lib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace lib.Factories;

public class DnsFactory : AzureResourceFactory
{
    private DnsFactoryConfig Config { get; }
    private long? DefaultTtl { get; } = (long?)TimeSpan.FromHours(1).TotalSeconds;

    public DnsFactory(ArmClientFactory clientFactory, IOptions<DnsFactoryConfig> config, ILogger<DnsFactory>? logger
    ) :
        base(clientFactory, config, logger)
    {
        Config = config.Value;
    }

    public async Task<DnsZoneResource> GetOrCreateDnsSubZoneAsync(string? subZoneName, string parentZoneName = null)
    {
        var existing = await GetDnsZoneAsync(subZoneName).ConfigureAwait(false);
        if (existing != null) return existing;

        var pzn = parentZoneName.Or(Config.ParentZoneName)
            .LogAndThrowWhenNull("Parent zone name is not set or not configured in config", logger: Logger);

        var parentZone = await
            GetDnsZoneAsync(pzn).ConfigureAwait(false);
        var rg = await GetResourceGroupAsync().ConfigureAwait(false);
        var fullSubZoneName = $"{subZoneName}.{pzn}";
        var subZoneData = new DnsZoneData(parentZone.Data.Location)
        {
            ZoneType = DnsZoneType.Public
        };

        existing = await rg.GetDnsZones().CreateOrUpdateAsync(WaitUntil.Completed, fullSubZoneName, subZoneData)
            .TriggerAndWaitAsync(logger: Logger);

        // Add NS record to parent zone
        var nsInfo = new DnsNSRecordData();
        foreach (var nameServer in existing.Data.NameServers)
        {
            nsInfo.DnsNSRecords.Add(new DnsNSRecordInfo
            {
                DnsNSDomainName = nameServer
            });
        }

        nsInfo.TtlInSeconds = DefaultTtl;

        var result = await parentZone.GetDnsNSRecords().CreateOrUpdateAsync(WaitUntil.Completed, subZoneName, nsInfo)
            .TriggerAndWaitAsync(
                preMessage: "Adding NS record to parent zone",
                successMessage: "NS record added to parent zone",
                failMessage: "Failed to add NS record to parent zone",
                logger: Logger);

        return existing;
    }


    public async Task<DnsZoneResource?> GetDnsZoneAsync(string? zoneName = null)
    {
        var zn = zoneName.Or(Config.ParentZoneName).LogAndThrowWhenNull(nameof(zoneName), logger: Logger);

        var rg = await GetResourceGroupAsync().ConfigureAwait(false);
        try
        {
            return await rg.GetDnsZoneAsync(zn).ConfigureAwait(false);
        }
        catch (RequestFailedException e)
        {
            if (e.Status == 404) return null;
            throw;
        }
    }

    public async Task<DnsTxtRecordResource> CreateTxtRecord(string entry, string value, string dnsZone = null)
    {
        var txtRecord = new DnsTxtRecordData();
        var dnsTxtRecordInfo = new DnsTxtRecordInfo();
        dnsTxtRecordInfo.Values.Add(value);
        txtRecord.DnsTxtRecords.Add(dnsTxtRecordInfo);
        txtRecord.TtlInSeconds = DefaultTtl;

        var dnsZoneName = dnsZone.Or(Config.ParentZoneName);

        var zone = await GetDnsZoneAsync(dnsZoneName).ConfigureAwait(false);
        var record = zone.GetDnsTxtRecords();
        var resource = await record.CreateOrUpdateAsync(WaitUntil.Completed, entry, txtRecord)
            .TriggerAndWaitAsync(logger: Logger);
        return resource;
    }

    public async Task<DnsCnameRecordResource> CreateCnameRecord(string entry, string value,
        string dnsZone = null)
    {
        var cnameRecord = new DnsCnameRecordData
        {
            Cname = value,
            TtlInSeconds = (long?)TimeSpan.FromHours(1).TotalSeconds
        };

        var dnsZoneName = dnsZone.Or(Config.ParentZoneName);

        var zone = await GetDnsZoneAsync(dnsZoneName).ConfigureAwait(false);
        var records = zone.GetDnsCnameRecords();
        var resource = await records.CreateOrUpdateAsync(WaitUntil.Completed, entry, cnameRecord)
            .TriggerAndWaitAsync(logger: Logger);

        return resource;
    }
}