using KeyVaultToAppConfig.Core.Secrets;
using KeyVaultToAppConfig.Services.Secrets;

namespace KeyVaultToAppConfig.UnitTests.Secrets;

public sealed class SecretRedactionTests
{
    [Fact]
    public void RedactName_ReturnsPlaceholderWhenEnabled()
    {
        var policy = new RedactionPolicy
        {
            RedactNames = true
        };
        var redaction = new SecretRedaction();

        var result = redaction.RedactName("secret-name", policy);

        Assert.Equal("[REDACTED]", result);
    }

    [Fact]
    public void RedactName_ReturnsOriginalWhenDisabled()
    {
        var policy = new RedactionPolicy
        {
            RedactNames = false
        };
        var redaction = new SecretRedaction();

        var result = redaction.RedactName("secret-name", policy);

        Assert.Equal("secret-name", result);
    }
}
