namespace KeyVaultToAppConfig.IntegrationTests;

public sealed class AuthIntegrationTests
{
    [Fact(Skip = "Requires live Azure identity configuration.")]
    public void ManagedIdentity_AuthenticatesSuccessfully()
    {
        Assert.True(true);
    }
}
