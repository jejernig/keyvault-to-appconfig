using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Auth;

namespace KeyVaultToAppConfig.UnitTests;

public sealed class CredentialResolutionTests
{
    [Fact]
    public void Policy_OrdersSourcesDeterministically()
    {
        var policy = new CredentialResolutionPolicy(new RunConfiguration
        {
            KeyVaultUri = "https://example.vault.azure.net",
            AppConfigEndpoint = "https://example.azconfig.io",
            ExecutionMode = ExecutionMode.DryRun
        });

        Assert.Equal(
            new[]
            {
                CredentialSource.ManagedIdentity,
                CredentialSource.WorkloadIdentity,
                CredentialSource.LocalDeveloper
            },
            policy.OrderedSources);
    }
}
