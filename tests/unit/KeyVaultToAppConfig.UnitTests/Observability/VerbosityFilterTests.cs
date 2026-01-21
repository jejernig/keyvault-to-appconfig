using KeyVaultToAppConfig.Core.Observability;
using KeyVaultToAppConfig.Services.Observability;

namespace KeyVaultToAppConfig.UnitTests.Observability;

public sealed class VerbosityFilterTests
{
    [Theory]
    [InlineData(VerbosityLevel.Minimal, VerbosityLevel.Minimal, true)]
    [InlineData(VerbosityLevel.Minimal, VerbosityLevel.Normal, false)]
    [InlineData(VerbosityLevel.Normal, VerbosityLevel.Minimal, true)]
    [InlineData(VerbosityLevel.Normal, VerbosityLevel.Normal, true)]
    [InlineData(VerbosityLevel.Normal, VerbosityLevel.Verbose, false)]
    [InlineData(VerbosityLevel.Verbose, VerbosityLevel.Verbose, true)]
    public void Allows_RespectsConfiguredLevel(
        VerbosityLevel configured,
        VerbosityLevel message,
        bool expected)
    {
        var filter = new VerbosityFilter();

        var result = filter.Allows(configured, message);

        Assert.Equal(expected, result);
    }
}
