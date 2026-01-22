using KeyVaultToAppConfig.Core.Errors;

namespace KeyVaultToAppConfig.UnitTests.ErrorHandling;

internal static class ErrorHandlingTestData
{
    internal static ErrorRecord CreateFatalRunError(string summary = "fatal")
    {
        return new ErrorRecord
        {
            ErrorId = Guid.NewGuid().ToString("N"),
            Classification = ErrorClassification.Fatal,
            Scope = "run",
            Stage = "startup",
            Summary = summary,
            OccurredAt = DateTimeOffset.UtcNow
        };
    }

    internal static ErrorRecord CreateRecoverableSecretError(string summary = "recoverable")
    {
        return new ErrorRecord
        {
            ErrorId = Guid.NewGuid().ToString("N"),
            Classification = ErrorClassification.Recoverable,
            Scope = "secret",
            Stage = "process",
            Summary = summary,
            OccurredAt = DateTimeOffset.UtcNow
        };
    }

    internal static SecretOperationOutcome CreateSecretOutcome(string key, SecretOutcomeStatus status, string? errorId = null)
    {
        return new SecretOperationOutcome
        {
            SecretKey = key,
            Status = status,
            ErrorId = errorId
        };
    }
}
