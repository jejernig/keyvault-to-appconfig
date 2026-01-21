namespace KeyVaultToAppConfig.Core.Planning;

public sealed class ExistingStateSnapshot
{
    public string SnapshotId { get; set; } = Guid.NewGuid().ToString("N");
    public List<ExistingEntry> Entries { get; set; } = new();
    public DateTimeOffset RetrievedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? KeyPrefix { get; set; }
    public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();
    public int? PageSize { get; set; }
    public string? ContinuationToken { get; set; }
}
