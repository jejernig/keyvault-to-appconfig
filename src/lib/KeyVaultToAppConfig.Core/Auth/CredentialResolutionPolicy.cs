namespace KeyVaultToAppConfig.Core.Auth;

public sealed class CredentialResolutionPolicy
{
    public IReadOnlyList<CredentialSource> OrderedSources { get; init; }
        = new List<CredentialSource>
        {
            CredentialSource.ManagedIdentity,
            CredentialSource.WorkloadIdentity,
            CredentialSource.LocalDeveloper
        };
}
