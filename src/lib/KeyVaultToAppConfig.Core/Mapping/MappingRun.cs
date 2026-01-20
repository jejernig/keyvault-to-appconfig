namespace KeyVaultToAppConfig.Core.Mapping;

public sealed class MappingRun
{
    public string RunId { get; set; } = Guid.NewGuid().ToString("N");
    public string SpecificationId { get; set; } = string.Empty;
    public string SpecificationVersion { get; set; } = string.Empty;
    public string? SourceSetId { get; set; }
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public MappingRunStatus Status { get; set; } = MappingRunStatus.Succeeded;
    public IReadOnlyDictionary<string, string> NormalizedKeys { get; set; } = new Dictionary<string, string>();
    public CollisionReport? CollisionReport { get; set; }
}

public enum MappingRunStatus
{
    Succeeded,
    Failed
}
