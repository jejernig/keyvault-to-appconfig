using KeyVaultToAppConfig.Core.Secrets;

namespace KeyVaultToAppConfig.Services.Secrets;

public interface ISecretModeEvaluator
{
    Task<ModeExecutionResult> EvaluateAsync(SecretModeRequest request, CancellationToken cancellationToken);
}
