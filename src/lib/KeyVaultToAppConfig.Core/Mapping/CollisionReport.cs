namespace KeyVaultToAppConfig.Core.Mapping;

public sealed class CollisionReport
{
    public List<CollisionEntry> Entries { get; set; } = new();
}

public sealed class CollisionEntry
{
    public string NormalizedKey { get; set; } = string.Empty;
    public List<string> SourceKeys { get; set; } = new();
    public CollisionPolicy AppliedPolicy { get; set; } = CollisionPolicy.Error;
}
