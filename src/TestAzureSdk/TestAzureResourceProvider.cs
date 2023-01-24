using System.Collections.Immutable;
using lib;
using lib.Configuration;
using Xunit.Abstractions;

namespace TestAzureSdk;

public class TestAzureResourceProvider : TestBaseWithArmClient
{
    private IAzureResourceProvider Target => GetService<IAzureResourceProvider>();
    private readonly CustomDomainConfig _config;

    public TestAzureResourceProvider(ITestOutputHelper testOutputHelperHelper) : base(testOutputHelperHelper)
    {
        _config = GetConfig<CustomDomainConfig>();
    }


    [Fact]
    public void GivenDiContainerIsReady_GettingResourceProvider_ReturnsValidInstance()
    {
        Assert.NotNull(_config);
        Assert.NotNull(_config.Dns);
        Assert.NotNull(_config.Frontdoor);

        Assert.NotNull(Target);
    }

    [Fact]
    public void GivenAzureResourceProviderExist_TryingToGetSubscription_ReturnsListOfSubscriptions()
    {
        RunWithEnvironment(() =>
        {
            var subscriptions = Target.ListSubscriptions();

            Assert.NotNull(subscriptions);
            Assert.NotEmpty(subscriptions);
            TestOutputHelper.WriteLine(string.Join(", ",
                subscriptions.Select(s => s.SubscriptionId)));
        });
    }

    [Fact]
    public void GivenConfigurationContainsFrondoorInformation_GettingItFromAzure_ReturnsTheMatchingInstance()
    {
        // Act
        RunWithEnvironment(() =>
        {
            var fd = Target.GetSubscription(_config.Frontdoor.SubscriptionId);
            var resources = fd.GetGenericResources()
                .ToImmutableList()
                .Select(r => new { r.Id, r.Data.Name, r.Id.ResourceType });
            
            var fd = Target.get
            Assert.NotNull(fd);
            TestOutputHelper.WriteLine(fd.Id);
        });
    }

    [Fact]
    public void GivenConfigurationContainsDnsInformation_GettingItFromAzure_ReturnsTheMatchingInstance()
    {
        throw new NotImplementedException();
    }
}