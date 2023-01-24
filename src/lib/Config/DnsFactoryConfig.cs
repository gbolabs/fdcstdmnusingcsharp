namespace lib.Config;

public class DnsFactoryConfig : IDescribeAzureResource
{
    public string? SubscriptionId { get; set; }
    public string? ResourceGroupName { get; set; }
    public string? DefaultLocation { get; set; }
    public string? ParentZoneName { get; set; }
}