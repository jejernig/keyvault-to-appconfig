using Azure.Identity;
using KeyVaultToAppConfig.Services;

namespace KeyVaultToAppConfig.UnitTests;

public sealed class AuthDiagnosticsTests
{
    [Fact]
    public void ValidateNoStaticSecrets_FlagsSecretPatterns()
    {
        var diagnostics = new AuthDiagnostics();

        var violations = diagnostics.ValidateNoStaticSecrets(new[] { "clientsecret=abc" });

        Assert.Single(violations);
    }

    [Fact]
    public void ValidateNoStaticSecrets_AllowsSafeValues()
    {
        var diagnostics = new AuthDiagnostics();

        var violations = diagnostics.ValidateNoStaticSecrets(new[] { "https://example" });

        Assert.Empty(violations);
    }

    [Fact]
    public void Classify_MapsAuthorizationFailuresToRbac()
    {
        var diagnostics = new AuthDiagnostics();
        var exception = new AuthenticationFailedException("authorization failed");

        var category = diagnostics.Classify(exception);

        Assert.Equal(KeyVaultToAppConfig.Core.Auth.AuthErrorCategory.Rbac, category);
    }

    [Fact]
    public void Classify_MapsGenericAuthFailuresToFatal()
    {
        var diagnostics = new AuthDiagnostics();
        var exception = new AuthenticationFailedException("no valid credential");

        var category = diagnostics.Classify(exception);

        Assert.Equal(KeyVaultToAppConfig.Core.Auth.AuthErrorCategory.Fatal, category);
    }
}
