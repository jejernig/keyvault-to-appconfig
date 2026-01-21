namespace KeyVaultToAppConfig.Core.Observability;

public static class ObservabilityExitCodes
{
    public const int Success = 0;
    public const int Failure = 1;

    public static int MapFromFailures(int failedCount)
    {
        return failedCount > 0 ? Failure : Success;
    }
}
