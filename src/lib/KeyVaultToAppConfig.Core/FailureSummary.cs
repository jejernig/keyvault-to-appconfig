namespace KeyVaultToAppConfig.Core;

public sealed class FailureSummary
{
    public string Key { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
