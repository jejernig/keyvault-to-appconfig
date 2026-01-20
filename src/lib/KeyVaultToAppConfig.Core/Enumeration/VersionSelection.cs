namespace KeyVaultToAppConfig.Core.Enumeration;

public sealed class VersionSelection
{
    public required VersionSelectionMode Mode { get; init; }
    public IDictionary<string, string>? ExplicitVersions { get; init; }
}

public enum VersionSelectionMode
{
    Latest,
    Explicit
}
