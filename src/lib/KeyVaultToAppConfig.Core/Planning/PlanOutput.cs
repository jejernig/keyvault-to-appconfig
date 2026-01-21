namespace KeyVaultToAppConfig.Core.Planning;

public sealed class PlanOutput
{
    public List<DiffItem> DiffItems { get; set; } = new();
    public List<ConflictRecord> Conflicts { get; set; } = new();
    public PlanTotals Totals { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
