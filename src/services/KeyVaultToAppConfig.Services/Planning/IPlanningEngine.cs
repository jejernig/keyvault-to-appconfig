using KeyVaultToAppConfig.Core.Planning;

namespace KeyVaultToAppConfig.Services.Planning;

public interface IPlanningEngine
{
    Task<PlanOutput> BuildPlanAsync(
        DesiredState desiredState,
        ExistingStateSnapshot existingState,
        CancellationToken cancellationToken);
}
