using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Observability;

namespace KeyVaultToAppConfig.UnitTests.ErrorHandling;

public sealed class ExitCodeMappingTests
{
    [Theory]
    [InlineData(RunOutcome.Success, ObservabilityExitCodes.Success)]
    [InlineData(RunOutcome.RecoverableFailures, ObservabilityExitCodes.RecoverableFailure)]
    [InlineData(RunOutcome.FatalFailure, ObservabilityExitCodes.FatalFailure)]
    [InlineData(RunOutcome.Canceled, ObservabilityExitCodes.Canceled)]
    public void MapFromOutcome_ReturnsExpectedCode(RunOutcome outcome, int expected)
    {
        var code = ObservabilityExitCodes.MapFromOutcome(outcome);

        Assert.Equal(expected, code);
    }
}
