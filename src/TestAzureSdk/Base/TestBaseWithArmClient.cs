using System.Diagnostics;
using System.Drawing.Printing;
using System.Net.Security;
using System.Text;

using Azure.ResourceManager;
using Azure.ResourceManager.Cdn;

using lib;
using lib.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Xunit.Abstractions;

namespace TestAzureSdk;

public abstract class TestBaseWithArmClient : ConfiguredBasedTest
{
    private readonly CustomDomainConfig _config;
    private readonly TestConfig _testConfig;


    protected TestBaseWithArmClient(ITestOutputHelper testOutputHelperHelper)
    {
        TestOutputHelper = testOutputHelperHelper;
        _config = Services.GetRequiredService<IOptions<CustomDomainConfig>>().Value;
        _testConfig = Services.GetRequiredService<IOptions<TestConfig>>().Value;
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        
        serviceCollection.AddAzureSdk(Configuration);
    }

    protected ITestOutputHelper TestOutputHelper { get; set; }

    [Fact]
    public void GivenDiContainerIsBuilt_BuildingThisClass_SetsAllConstructorDependencies()
    {
        Assert.NotNull(this); // Constructor would fail to be invoked given the DI Container would not be ready

        Assert.NotNull(TestOutputHelper);
        Assert.NotNull(_config);
        
        Assert.NotNull(_testConfig);
        Assert.False(string.IsNullOrWhiteSpace(_testConfig.Name));
        Assert.NotEmpty(_testConfig.EnvironmentVariables);
    }

    [Fact]
    public void EnsureSdkCredentialsEnvironmentVariables()
    {
        // Arrange
        SetupEnvironmentVariables();
        
        Assert.NotNull(Environment.GetEnvironmentVariable(TestConfig.AzureClientId));
        Assert.NotNull(Environment.GetEnvironmentVariable(TestConfig.AzureTenantId));
        Assert.NotNull(Environment.GetEnvironmentVariable(TestConfig.AzureClientSecret));
    }

    protected virtual void SetupEnvironmentVariables()
    {
        foreach (var environmentVariable in _testConfig.EnvironmentVariables)
        {
            Environment.SetEnvironmentVariable(environmentVariable.Key,
                environmentVariable.Value);
        }
    }

    protected void RunWithEnvironment(Action testMethod)
    {
        // Setup environment variables
        SetupEnvironmentVariables();
        
        // Run method
        testMethod.Invoke();
    }
}