using KeyVaultToAppConfig.Core.Writes;

namespace KeyVaultToAppConfig.Services.Writes;

public sealed class WriteOptions
{
    public int MaxParallelism { get; set; } = 4;
    public RetryPolicy RetryPolicy { get; set; } = new();
    public RollbackPlan? RollbackPlan { get; set; }
}
