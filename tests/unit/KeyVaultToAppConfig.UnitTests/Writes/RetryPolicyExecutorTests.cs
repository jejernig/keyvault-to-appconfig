using KeyVaultToAppConfig.Core.Writes;
using KeyVaultToAppConfig.Services.Writes;

namespace KeyVaultToAppConfig.UnitTests.Writes;

public sealed class RetryPolicyExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_RetriesAndSucceeds()
    {
        var action = new WriteAction { Key = "alpha", Label = "prod", ActionType = WriteActionType.Update };
        var policy = new RetryPolicy { MaxAttempts = 2, BaseDelaySeconds = 1, MaxDelaySeconds = 1 };
        var executor = new RetryPolicyExecutor();
        var attempts = 0;

        var result = await executor.ExecuteAsync(
            action,
            policy,
            _ =>
            {
                attempts++;
                if (attempts == 1)
                {
                    throw new InvalidOperationException("transient");
                }

                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.Equal(WriteStatus.Succeeded, result.Status);
        Assert.Equal(2, result.Attempts);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailureAfterRetries()
    {
        var action = new WriteAction { Key = "beta", Label = "prod", ActionType = WriteActionType.Update };
        var policy = new RetryPolicy { MaxAttempts = 2, BaseDelaySeconds = 1, MaxDelaySeconds = 1 };
        var executor = new RetryPolicyExecutor();

        var result = await executor.ExecuteAsync(
            action,
            policy,
            _ => throw new InvalidOperationException("fail"),
            CancellationToken.None);

        Assert.Equal(WriteStatus.Failed, result.Status);
        Assert.Equal(2, result.Attempts);
    }
}
