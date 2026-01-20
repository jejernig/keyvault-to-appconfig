using System.Reflection;
using KeyVaultToAppConfig.Services;

namespace KeyVaultToAppConfig.UnitTests;

public sealed class AuthRetryPolicyTests
{
    [Fact]
    public void AuthResolver_UsesBoundedRetryDefaults()
    {
        var maxAttemptsField = typeof(AuthResolver)
            .GetField("MaxAttempts", BindingFlags.NonPublic | BindingFlags.Static);
        var retryWindowField = typeof(AuthResolver)
            .GetField("RetryWindow", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(maxAttemptsField);
        Assert.NotNull(retryWindowField);

        var maxAttempts = (int)maxAttemptsField!.GetRawConstantValue()!;
        var retryWindow = (TimeSpan)retryWindowField!.GetValue(null)!;

        Assert.Equal(3, maxAttempts);
        Assert.Equal(TimeSpan.FromSeconds(10), retryWindow);
    }
}
