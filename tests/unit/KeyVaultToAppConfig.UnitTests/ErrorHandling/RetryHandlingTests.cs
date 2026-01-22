using KeyVaultToAppConfig.Core.Writes;
using KeyVaultToAppConfig.Services.Writes;

namespace KeyVaultToAppConfig.UnitTests.ErrorHandling;

public sealed class RetryHandlingTests
{
    [Fact]
    public async Task ExecuteAsync_RespectsCancellation()
    {
        var executor = new RetryPolicyExecutor();
        var action = new WriteAction { Key = "k1", ActionType = WriteActionType.Create };
        var policy = new RetryPolicy { MaxAttempts = 3, BaseDelaySeconds = 1, MaxDelaySeconds = 2 };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            executor.ExecuteAsync(action, policy, _ => Task.CompletedTask, cts.Token));
    }
}
