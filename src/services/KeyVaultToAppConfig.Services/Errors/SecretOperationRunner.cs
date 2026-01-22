using KeyVaultToAppConfig.Core.Errors;
using KeyVaultToAppConfig.Core.Enumeration;

namespace KeyVaultToAppConfig.Services.Errors;

public sealed class SecretOperationRunner
{
    private readonly ErrorClassifier _classifier;

    public SecretOperationRunner(ErrorClassifier classifier)
    {
        _classifier = classifier ?? throw new ArgumentNullException(nameof(classifier));
    }

    public async Task<SecretOperationRunResult> ExecuteAsync(
        IEnumerable<SecretDescriptor> secrets,
        bool failFast,
        Func<SecretDescriptor, CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        if (secrets is null)
        {
            throw new ArgumentNullException(nameof(secrets));
        }

        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        var secretList = secrets.ToList();
        var result = new SecretOperationRunResult();
        var canceled = false;
        foreach (var secret in secretList)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await operation(secret, cancellationToken);
                result.Outcomes.Add(new SecretOperationOutcome
                {
                    SecretKey = secret.Name,
                    Status = SecretOutcomeStatus.Success
                });
            }
            catch (OperationCanceledException)
            {
                canceled = true;
                break;
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid().ToString("N");
                var classification = _classifier.Classify("secret", ex);
                result.Errors.Add(new ErrorRecord
                {
                    ErrorId = errorId,
                    Classification = classification,
                    Scope = "secret",
                    Stage = "process",
                    Summary = ex.Message,
                    OccurredAt = DateTimeOffset.UtcNow
                });

                result.Outcomes.Add(new SecretOperationOutcome
                {
                    SecretKey = secret.Name,
                    Status = SecretOutcomeStatus.RecoverableFailure,
                    ErrorId = errorId
                });

                if (failFast)
                {
                    break;
                }
            }
        }

        if ((failFast || canceled) && result.Outcomes.Count < secretList.Count)
        {
            foreach (var remaining in secretList.Skip(result.Outcomes.Count))
            {
                result.Outcomes.Add(new SecretOperationOutcome
                {
                    SecretKey = remaining.Name,
                    Status = SecretOutcomeStatus.Unprocessed
                });
            }
        }

        result.WasCanceled = canceled;
        return result;
    }
}

public sealed class SecretOperationRunResult
{
    public List<SecretOperationOutcome> Outcomes { get; } = new();
    public List<ErrorRecord> Errors { get; } = new();
    public bool WasCanceled { get; set; }
}
