namespace KeyVaultToAppConfig.Core.Writes;

public sealed class WriteAction
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public WriteActionType ActionType { get; set; }
    public string? DesiredValue { get; set; }
    public string? DesiredContentType { get; set; }
    public string? Reason { get; set; }
}
