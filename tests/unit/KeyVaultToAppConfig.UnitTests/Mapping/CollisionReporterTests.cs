using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Services.Mapping;

namespace KeyVaultToAppConfig.UnitTests.Mapping;

public sealed class CollisionReporterTests
{
    [Fact]
    public void Register_CreatesCollisionEntry()
    {
        var reporter = new CollisionReporter();

        reporter.Register("normalized", "sourceA", "sourceB", CollisionPolicy.KeepFirst);
        var report = reporter.BuildReport();

        Assert.NotNull(report);
        Assert.Single(report!.Entries);
        Assert.Equal("normalized", report.Entries[0].NormalizedKey);
        Assert.Contains("sourceA", report.Entries[0].SourceKeys);
        Assert.Contains("sourceB", report.Entries[0].SourceKeys);
        Assert.Equal(CollisionPolicy.KeepFirst, report.Entries[0].AppliedPolicy);
    }
}
