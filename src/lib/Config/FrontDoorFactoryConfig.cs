namespace lib.Config;

public class FrontDoorFactoryConfig : IDescribeAzureResource

{
    public string? SubscriptionId { get; set; }
    public string? ResourceGroupName { get; set; }
    public string? DefaultLocation { get; set; }
    public string FrontDoorName { get; set; }
}