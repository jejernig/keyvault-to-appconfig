namespace KeyVaultToAppConfig.Core.Planning;

public sealed class ConflictRecord
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<string> ConflictingValues { get; set; } = new();
    public string ResolutionStatus { get; set; } = string.Empty;
}
