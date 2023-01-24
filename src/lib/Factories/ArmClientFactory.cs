using Azure.Identity;
using Azure.ResourceManager;
using lib.Config;
using Microsoft.Extensions.Options;

namespace lib.Factories;

public class ArmClientFactory
{
    private readonly ArmClientFactoryConfig _config;
    private readonly Lazy<ArmClient> _lazyClient;

    public ArmClientFactory(IOptions<ArmClientFactoryConfig> config)
    {
        _config = config.Value;
        _lazyClient = new Lazy<ArmClient>(BuildClient);
    }

    private ArmClient BuildClient()
    {
        var credentialType = _config.CredentialType ?? string.Empty;
        if (credentialType.Equals(nameof(ManagedIdentityCredential), StringComparison.OrdinalIgnoreCase))
        {
            return new ArmClient(new ManagedIdentityCredential());
        }
        if (credentialType.Equals(nameof(EnvironmentCredential), StringComparison.OrdinalIgnoreCase))
        {
            return new ArmClient(new EnvironmentCredential());
        }

        var client = new ArmClient(new DefaultAzureCredential());
        return client;
    }

    public ArmClient Client => _lazyClient.Value;
}