namespace KeyVaultToAppConfig.Core.Writes;

public sealed class WritePlan
{
    public string PlanId { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<WriteAction> Actions { get; set; } = new();
    public LabelContext? LabelContext { get; set; }
    public ManagedMetadata? ManagedMetadata { get; set; }
    public RollbackPlan? RollbackPlan { get; set; }
}
