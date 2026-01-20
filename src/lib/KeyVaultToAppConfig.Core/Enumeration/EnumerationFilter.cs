namespace KeyVaultToAppConfig.Core.Enumeration;

public sealed class EnumerationFilter
{
    public string? Prefix { get; init; }
    public string? Regex { get; init; }
    public IDictionary<string, string>? Tags { get; init; }
    public bool EnabledOnly { get; init; }
}
