using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Core.Writes;

namespace KeyVaultToAppConfig.Services.Writes;

public interface IWritePlanner
{
    Task<WritePlan> BuildPlanAsync(
        DesiredState desiredState,
        ExistingStateSnapshot existingState,
        LabelContext labelContext,
        ManagedMetadata managedMetadata,
        CancellationToken cancellationToken);
}
