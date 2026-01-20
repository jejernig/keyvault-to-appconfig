namespace KeyVaultToAppConfig.Core;

public sealed class ChangeSummary
{
    public string Key { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
