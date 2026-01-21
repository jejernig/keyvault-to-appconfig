namespace KeyVaultToAppConfig.Core.Writes;

public sealed class RollbackPlan
{
    public bool Enabled { get; set; }
    public string? SnapshotId { get; set; }
    public List<WriteAction> Actions { get; set; } = new();
}
