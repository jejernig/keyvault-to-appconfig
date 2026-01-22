using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.Core.Observability;

public static class ObservabilityExitCodes
{
    public const int Success = 0;
    public const int RecoverableFailure = 1;
    public const int FatalFailure = 2;
    public const int Canceled = 3;

    public static int MapFromFailures(int failedCount)
    {
        return failedCount > 0 ? RecoverableFailure : Success;
    }

    public static int MapFromOutcome(RunOutcome outcome)
    {
        return outcome switch
        {
            RunOutcome.Success => Success,
            RunOutcome.RecoverableFailures => RecoverableFailure,
            RunOutcome.FatalFailure => FatalFailure,
            RunOutcome.Canceled => Canceled,
            _ => FatalFailure
        };
    }
}
