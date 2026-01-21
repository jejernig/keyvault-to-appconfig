using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Core.Writes;
using KeyVaultToAppConfig.Services.Writes;

namespace KeyVaultToAppConfig.UnitTests.Writes;

public sealed class WritePlannerTests
{
    [Fact]
    public async Task BuildPlanAsync_SkipsUnchangedEntries()
    {
        var desiredState = new DesiredState
        {
            Entries =
            {
                new DesiredEntry { Key = "alpha", Label = "prod", Value = "same" }
            }
        };

        var existingState = new ExistingStateSnapshot
        {
            Entries =
            {
                new ExistingEntry { Key = "alpha", Label = "prod", Value = "same" }
            }
        };

        var planner = new WritePlanner();
        var plan = await planner.BuildPlanAsync(
            desiredState,
            existingState,
            new LabelContext(),
            new ManagedMetadata(),
            CancellationToken.None);

        Assert.Single(plan.Actions);
        Assert.Equal(WriteActionType.Skip, plan.Actions[0].ActionType);
    }

    [Fact]
    public async Task BuildPlanAsync_CreatesWhenMissing()
    {
        var desiredState = new DesiredState
        {
            Entries =
            {
                new DesiredEntry { Key = "beta", Label = "prod", Value = "new" }
            }
        };

        var planner = new WritePlanner();
        var plan = await planner.BuildPlanAsync(
            desiredState,
            new ExistingStateSnapshot(),
            new LabelContext(),
            new ManagedMetadata(),
            CancellationToken.None);

        Assert.Equal(WriteActionType.Create, plan.Actions[0].ActionType);
    }

    [Fact]
    public async Task BuildPlanAsync_SkipsConflicts()
    {
        var desiredState = new DesiredState
        {
            Entries =
            {
                new DesiredEntry { Key = "dup", Label = "prod", Value = "v1" },
                new DesiredEntry { Key = "dup", Label = "prod", Value = "v2" }
            }
        };

        var planner = new WritePlanner();
        var plan = await planner.BuildPlanAsync(
            desiredState,
            new ExistingStateSnapshot(),
            new LabelContext(),
            new ManagedMetadata(),
            CancellationToken.None);

        Assert.All(plan.Actions, action => Assert.Equal(WriteActionType.Skip, action.ActionType));
    }
}
