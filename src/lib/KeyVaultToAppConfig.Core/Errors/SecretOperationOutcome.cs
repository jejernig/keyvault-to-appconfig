namespace KeyVaultToAppConfig.Core.Errors;

public sealed class SecretOperationOutcome
{
    public string SecretKey { get; set; } = string.Empty;
    public SecretOutcomeStatus Status { get; set; }
    public string? ErrorId { get; set; }
}

public enum SecretOutcomeStatus
{
    Success,
    RecoverableFailure,
    Unprocessed
}
