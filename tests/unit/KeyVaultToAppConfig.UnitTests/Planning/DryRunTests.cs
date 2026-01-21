using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Services.Planning;

namespace KeyVaultToAppConfig.UnitTests.Planning;

public sealed class DryRunTests
{
    [Fact]
    public async Task ExecuteAsync_MatchesPlanningOutput()
    {
        var desiredState = new DesiredState
        {
            Entries =
            {
                new DesiredEntry { Key = "alpha", Label = "prod", Value = "value" }
            }
        };

        var existingState = new ExistingStateSnapshot();
        var planningEngine = new PlanningEngine();
        var handler = new DryRunHandler(planningEngine);

        var plan = await planningEngine.BuildPlanAsync(desiredState, existingState, CancellationToken.None);
        var dryRun = await handler.ExecuteAsync(desiredState, existingState, CancellationToken.None);

        Assert.Equal(
            plan.DiffItems.Select(item => item.Classification),
            dryRun.DiffItems.Select(item => item.Classification));
        Assert.Equal(plan.Conflicts.Count, dryRun.Conflicts.Count);
    }

    [Fact]
    public async Task ExecuteAsync_UsesPlanningEngineOnce()
    {
        var fakeEngine = new FakePlanningEngine();
        var handler = new DryRunHandler(fakeEngine);

        _ = await handler.ExecuteAsync(new DesiredState(), new ExistingStateSnapshot(), CancellationToken.None);

        Assert.Equal(1, fakeEngine.CallCount);
    }

    private sealed class FakePlanningEngine : IPlanningEngine
    {
        public int CallCount { get; private set; }

        public Task<PlanOutput> BuildPlanAsync(
            DesiredState desiredState,
            ExistingStateSnapshot existingState,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new PlanOutput());
        }
    }
}
