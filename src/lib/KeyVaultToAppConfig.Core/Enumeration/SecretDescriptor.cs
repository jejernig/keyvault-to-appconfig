namespace KeyVaultToAppConfig.Core.Enumeration;

public sealed class SecretDescriptor
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public bool IsEnabled { get; init; }
    public IDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
    public DateTimeOffset LastUpdated { get; init; }
}
