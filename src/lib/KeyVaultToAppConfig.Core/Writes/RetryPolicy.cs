namespace KeyVaultToAppConfig.Core.Writes;

public sealed class RetryPolicy
{
    public int MaxAttempts { get; set; } = 3;
    public int BaseDelaySeconds { get; set; } = 1;
    public int MaxDelaySeconds { get; set; } = 10;
}
