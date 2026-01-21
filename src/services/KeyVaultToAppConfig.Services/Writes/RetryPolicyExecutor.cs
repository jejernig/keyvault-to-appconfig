using KeyVaultToAppConfig.Core.Writes;

namespace KeyVaultToAppConfig.Services.Writes;

public sealed class RetryPolicyExecutor
{
    public async Task<WriteResult> ExecuteAsync(
        WriteAction action,
        RetryPolicy retryPolicy,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (retryPolicy is null)
        {
            throw new ArgumentNullException(nameof(retryPolicy));
        }

        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        var attempts = 0;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= retryPolicy.MaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempts = attempt;

            try
            {
                await operation(cancellationToken);
                return new WriteResult
                {
                    Key = action.Key,
                    Label = action.Label,
                    Status = WriteStatus.Succeeded,
                    Attempts = attempts,
                    RetryCount = attempts - 1
                };
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt == retryPolicy.MaxAttempts)
                {
                    break;
                }

                var delaySeconds = Math.Min(
                    retryPolicy.MaxDelaySeconds,
                    retryPolicy.BaseDelaySeconds * (int)Math.Pow(2, attempt - 1));
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
        }

        return new WriteResult
        {
            Key = action.Key,
            Label = action.Label,
            Status = WriteStatus.Failed,
            Attempts = attempts,
            RetryCount = Math.Max(0, attempts - 1),
            FailureReason = lastException?.Message
        };
    }
}
