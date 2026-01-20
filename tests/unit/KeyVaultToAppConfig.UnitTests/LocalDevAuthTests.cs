using KeyVaultToAppConfig.Services;

namespace KeyVaultToAppConfig.UnitTests;

public sealed class LocalDevAuthTests
{
    [Fact]
    public void LocalDevInputs_DoNotTriggerStaticSecretDetection()
    {
        var diagnostics = new AuthDiagnostics();

        var violations = diagnostics.ValidateNoStaticSecrets(new[] { "use-azure-cli", "localhost" });

        Assert.Empty(violations);
    }
}
