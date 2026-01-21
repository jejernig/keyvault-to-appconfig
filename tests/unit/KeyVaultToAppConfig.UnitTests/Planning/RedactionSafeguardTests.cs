using KeyVaultToAppConfig.Services.Planning;

namespace KeyVaultToAppConfig.UnitTests.Planning;

public sealed class RedactionSafeguardTests
{
    [Fact]
    public void RedactMessage_RemovesSecretValues()
    {
        var input = "value=supersecret password=hunter2";

        var output = PlanningLogging.RedactMessage(input);

        Assert.DoesNotContain("supersecret", output);
        Assert.DoesNotContain("hunter2", output);
        Assert.Contains("[REDACTED]", output);
    }
}
