namespace lib.Config;

public interface IDescribeAzureResource
{
    string? SubscriptionId { get; set; }
    string? ResourceGroupName { get; set; }
    string? DefaultLocation { get; set; }
}