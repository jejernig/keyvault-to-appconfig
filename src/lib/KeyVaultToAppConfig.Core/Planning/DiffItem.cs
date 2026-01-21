namespace KeyVaultToAppConfig.Core.Planning;

public sealed class DiffItem
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DiffClassification Classification { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DesiredValue { get; set; }
    public string? ExistingValue { get; set; }
    public string? DesiredContentType { get; set; }
    public string? ExistingContentType { get; set; }
}
