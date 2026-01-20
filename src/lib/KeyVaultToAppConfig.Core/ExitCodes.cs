namespace KeyVaultToAppConfig.Core;

public static class ExitCodes
{
    public const int SuccessNoChanges = 0;
    public const int SuccessWithChanges = 1;
    public const int PartialFailures = 2;
    public const int FatalError = 3;
}
