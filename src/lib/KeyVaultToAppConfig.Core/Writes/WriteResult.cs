namespace KeyVaultToAppConfig.Core.Writes;

public sealed class WriteResult
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public WriteStatus Status { get; set; }
    public int Attempts { get; set; }
    public int RetryCount { get; set; }
    public string? FailureReason { get; set; }
}
