using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.UnitTests.Mapping;

public sealed class RedactionSafeguardTests
{
    [Fact]
    public void Redact_RemovesSecretValues()
    {
        var input = "value=supersecret password=hidden";
        var redacted = Redaction.Redact(input);

        Assert.Contains("value=[REDACTED]", redacted);
        Assert.Contains("password=[REDACTED]", redacted);
        Assert.DoesNotContain("supersecret", redacted);
        Assert.DoesNotContain("hidden", redacted);
    }
}
