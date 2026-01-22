namespace KeyVaultToAppConfig.Core.Errors;

public sealed class ErrorRecord
{
    public string ErrorId { get; set; } = string.Empty;
    public ErrorClassification Classification { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
