namespace KeyVaultToAppConfig.Core.Planning;

public sealed class DesiredEntry
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? SourceId { get; set; }
}
