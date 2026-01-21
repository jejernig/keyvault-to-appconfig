using KeyVaultToAppConfig.Core.Observability;

namespace KeyVaultToAppConfig.Services.Observability;

public sealed class VerbosityFilter
{
    public bool Allows(VerbosityLevel configured, VerbosityLevel messageLevel)
    {
        return messageLevel <= configured;
    }
}
