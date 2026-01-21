namespace KeyVaultToAppConfig.Core.Writes;

public sealed class ManagedMetadata
{
    public string? Source { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public IDictionary<string, string> AdditionalTags { get; set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
