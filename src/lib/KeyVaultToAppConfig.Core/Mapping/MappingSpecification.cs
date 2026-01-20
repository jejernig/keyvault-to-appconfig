namespace KeyVaultToAppConfig.Core.Mapping;

public sealed class MappingSpecification
{
    public string SpecificationId { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MappingDefaultBehavior DefaultBehavior { get; set; } = MappingDefaultBehavior.RejectUnmapped;
    public CollisionPolicy CollisionPolicy { get; set; } = CollisionPolicy.Error;
    public List<MappingRule> Rules { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
}

public enum MappingDefaultBehavior
{
    RejectUnmapped,
    PassThrough
}

public enum CollisionPolicy
{
    Error,
    KeepFirst,
    KeepLast,
    ReportOnly
}
