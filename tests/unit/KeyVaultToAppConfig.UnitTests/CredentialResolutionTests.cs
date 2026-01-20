using KeyVaultToAppConfig.Core.Auth;

namespace KeyVaultToAppConfig.UnitTests;

public sealed class CredentialResolutionTests
{
    [Fact]
    public void Policy_OrdersSourcesDeterministically()
    {
        var policy = new CredentialResolutionPolicy();

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
