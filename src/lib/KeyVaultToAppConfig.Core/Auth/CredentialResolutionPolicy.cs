namespace KeyVaultToAppConfig.Core.Auth;

public sealed class CredentialResolutionPolicy
{
    public CredentialResolutionPolicy(RunConfiguration config)
    {
        var sources = new List<CredentialSource>();

        if (!config.DisableManagedIdentity)
        {
            sources.Add(CredentialSource.ManagedIdentity);
        }

        if (!config.DisableWorkloadIdentity)
        {
            sources.Add(CredentialSource.WorkloadIdentity);
        }

        if (!(config.DisableAzureCli && config.DisableVisualStudio && config.DisableVisualStudioCode))
        {
            sources.Add(CredentialSource.LocalDeveloper);
        }

        OrderedSources = sources;
    }

    public IReadOnlyList<CredentialSource> OrderedSources { get; }
}
