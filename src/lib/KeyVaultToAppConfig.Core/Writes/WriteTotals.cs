namespace KeyVaultToAppConfig.Core.Writes;

public sealed class WriteTotals
{
    public int CreateCount { get; set; }
    public int UpdateCount { get; set; }
    public int SkipCount { get; set; }
    public int FailedCount { get; set; }
}
