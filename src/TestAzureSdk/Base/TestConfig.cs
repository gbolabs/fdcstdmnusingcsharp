using Azure.Core.Extensions;

namespace TestAzureSdk;

public class TestConfig
{
    internal const string AzureClientId = "AZURE_CLIENT_ID";
    internal const string AzureTenantId = "AZURE_TENANT_ID";
    internal const string AzureClientSecret = "AZURE_CLIENT_SECRET";

    public string Name { get; set; } = string.Empty;
    
    public EnvironmentVariableConfig[] EnvironmentVariables { get; set; }
}

public class EnvironmentVariableConfig
{
    public string Key { get; set; }
    public string Value { get; set; }
}