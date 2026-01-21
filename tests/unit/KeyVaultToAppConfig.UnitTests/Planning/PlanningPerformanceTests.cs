using System.Diagnostics;
using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Services.Planning;

namespace KeyVaultToAppConfig.UnitTests.Planning;

public sealed class PlanningPerformanceTests
{
    [Fact]
    public async Task BuildPlanAsync_HandlesTenThousandEntriesUnderFiveSeconds()
    {
        var entries = Enumerable.Range(0, 10_000)
            .Select(i => new DesiredEntry
            {
                Key = $"key-{i:D5}",
                Label = "prod",
                Value = $"value-{i:D5}"
            })
            .ToList();

        var desiredState = new DesiredState { Entries = entries };
        var existingState = new ExistingStateSnapshot();
        var engine = new PlanningEngine();

        var stopwatch = Stopwatch.StartNew();
        _ = await engine.BuildPlanAsync(desiredState, existingState, CancellationToken.None);
        stopwatch.Stop();

        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromSeconds(5),
            $"Planning 10k entries exceeded 5 seconds. Elapsed: {stopwatch.Elapsed.TotalSeconds:F2}s");
    }
}
