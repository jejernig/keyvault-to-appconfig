using KeyVaultToAppConfig.Core.Planning;

namespace KeyVaultToAppConfig.Services.Planning;

public sealed class DryRunHandler
{
    private readonly IPlanningEngine _planningEngine;

    public DryRunHandler(IPlanningEngine planningEngine)
    {
        _planningEngine = planningEngine ?? throw new ArgumentNullException(nameof(planningEngine));
    }

    public Task<PlanOutput> ExecuteAsync(
        DesiredState desiredState,
        ExistingStateSnapshot existingState,
        CancellationToken cancellationToken)
    {
        return _planningEngine.BuildPlanAsync(desiredState, existingState, cancellationToken);
    }
}
