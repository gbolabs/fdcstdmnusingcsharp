# TIV Custom Domain Creator

## Objective

With the code contains within the present repository we want to achieve the creation and management of Azure Resources out of an application.

## Affected resources

The main objective is to create alternative custom domain for already existing Frontdoor Endpoints.

## Content

This repository contains C# code-fragments using a `Lib`-project ready to be integrated within another .net 6+ application.

A NuGet Package might be forseen in a coming feature to ease the integration of this library.

## Requirements

The following point must be configured and setup to ensure a proper execution:

- A _.NET6.0_ or newer application
- A running Service Dependency Injection container (`Microsoft.Extensions.DependencyInjection`)
- Configuration must be added to the application's configuration file (`Microsoft.Extensions.Configuration`)
- Identity
  - When hosted in Azure; the executing resource must be assigned a Managed Identity granted the required permission (see below).
  - When running in Visual Studio; one of the following method must be configured. The logged-in identity must share the permissions as above.
    - Environment Variables
    - An existing _AZ CLI_ login (using `az login -t {tenant}`)
    - VisualStudio or VisualStudioCode is connected with the tenant

### Azure Permissions

To follow the _least-priviledges-principle the following RBAC Roles are required to achieve the forseen operations:

- `DNS Zone Contributor` applied at the resource-group level if we expect to create new ressources otherwise it can be applied on the configured `ParentZoneName` resource.
- `CDN Profile Contributor` applied on the Azure FrontDoor Resources involved.

Those permissions must be associated with the ManagedIdentity associated with the Azure Resource (either _System-assigned_ or _User-assigned_) and, when needed, with the _Service Principal_ used through environment variables. When, for development purposes, a user-login is being this person must be granted the same priviledges.

More information:

- Create _Service Principal_: <https://learn.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli#password-based-authentication>
- All details about Azure SDK Authentication:
  - <https://devblogs.microsoft.com/azure-sdk/authentication-and-the-azure-sdk/>
  - <https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication?tabs=command-line>

## How-to-use

1. Copy the whole `lib`-project within your application. Later-on a NuGet package could arise.
1. Extend your application configuration with the following entries.

   ```json
     "FrontDoorFactoryConfig": {
        "FrontDoorName": "fd-lab-01",
        "ResourceGroupName": "rg-frontdoor-cmn",
        "SubscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
    },
    "DnsFactoryConfig": {
        "ParentZoneName": "myzone.mydomain.net",
        "ResourceGroupName": "rg-dns-cmn",
        "SubscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
    }
   ```

1. Register the required Services and Configuration entries within your DI-Container using the following method.

    ```csharp
    using lib;
    ...
    ServiceCollection().AddAzureAdapters(configuration)
    ...
    ```

1. Retrieve from the DI (using `IServiceProvider`) an prepared instance of `CreateCustomHostNameJob`

   ```csharp
   // By invoking the service provider directly
   var job = serviceProvider.GetRequiredService<CreateCustomHostNameJob>();

   // By using Constructor injection
   class MyClass()
   {
    public MyClass(CreateCustomHostNameJob job){
        ...
    }
   }
   ```

1. Invoke one or the other method from the retrieved Job

   ```csharp
   // Create a new custom domain within the default, configured using ParentZoneName, dns-zone
   var frontDoorResource = await job.CreateCustomDomainWithSubZoneAsync("test04", "appzone", "statapp2").ConfigureAwait(false);

   // Create a new custom domain within another dns sub-zone, to be eventually created, within the configured ParentZoneName.
   var frontDoorResource = await job.CreateCustomDomainAsync("test05", "statapp2").ConfigureAwait(false);

   ```

### Configuration

The configuration requires the following entries, a sample configuration file can be found in the repo (_appsettings.sample.json_).

```json
"ArmClientFactoryConfig":
{
    "CredentialType": "ManagedIdentityCredential", // Will use the execution-context associated Managed Identity. Designed for execution in Azure Resources (AppServices, Container...)
    "CredentialType": "EnvironmentCredential", // Will search for the three environment variables below within the process' environment variables. Aimed for execution outside Azure native resources.
    "CredentialType": null // Will relay on the DefaultAzureCredential class cascaiding through all the support methods. Aimed for developing scenarios.
},
     "FrontDoorFactoryConfig": {
        "FrontDoorName": "fd-lab-01",
        "ResourceGroupName": "rg-frontdoor-cmn",
        "SubscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
    },
    "DnsFactoryConfig": {
        "ParentZoneName": "myzone.mydomain.net",
        "ResourceGroupName": "rg-dns-cmn",
        "SubscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
    }
"
```

### Environment Variables

When using the `"CredentialType": "EnvironmentCredential"` configuration the following variables must be present within the process' environment variables list.

|Variable|Value|
|:-|:-|
|`AZURE_CLIENT_ID`||
|`AZURE_CLIENT_SECRET`||
|`AZURE_TENANT_ID`|_Tenant Id issuing the above client_|

## How-to-build

- From the source, use either the `lib.csproj` project-file or `custom-domain-using-csharp.sln` solution-file in your favorite IDE.
