namespace KeyVaultToAppConfig.Core.Planning;

public sealed class PlanTotals
{
    public int CreateCount { get; set; }
    public int UpdateCount { get; set; }
    public int UnchangedCount { get; set; }
    public int ConflictCount { get; set; }
}
