using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Services.Planning;

namespace KeyVaultToAppConfig.UnitTests.Planning;

public sealed class ConflictDetectionTests
{
    [Fact]
    public async Task BuildPlanAsync_DetectsConflictingDesiredEntries()
    {
        var desiredState = new DesiredState
        {
            Entries =
            {
                new DesiredEntry { Key = "alpha", Label = "prod", Value = "v1" },
                new DesiredEntry { Key = "alpha", Label = "prod", Value = "v2" }
            }
        };

        var engine = new PlanningEngine();
        var plan = await engine.BuildPlanAsync(desiredState, new ExistingStateSnapshot(), CancellationToken.None);

        Assert.Single(plan.Conflicts);
        Assert.Empty(plan.DiffItems);
        Assert.Contains("v1", plan.Conflicts[0].ConflictingValues);
        Assert.Contains("v2", plan.Conflicts[0].ConflictingValues);
    }
}
