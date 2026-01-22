using KeyVaultToAppConfig.Core.Observability;

namespace KeyVaultToAppConfig.UnitTests.Observability;

public sealed class ExitCodeTests
{
    [Theory]
    [InlineData(0, ObservabilityExitCodes.Success)]
    [InlineData(1, ObservabilityExitCodes.RecoverableFailure)]
    [InlineData(5, ObservabilityExitCodes.RecoverableFailure)]
    public void MapFromFailures_UsesContract(int failures, int expected)
    {
        var result = ObservabilityExitCodes.MapFromFailures(failures);

        Assert.Equal(expected, result);
    }
}
