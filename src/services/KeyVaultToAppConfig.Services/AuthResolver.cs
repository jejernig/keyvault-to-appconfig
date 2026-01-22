using Azure.Core;
using Azure.Identity;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Auth;

namespace KeyVaultToAppConfig.Services;

public sealed class AuthResolver
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan RetryWindow = TimeSpan.FromSeconds(10);

    private readonly AuthDiagnostics _diagnostics;
    private readonly RunConfiguration _config;

    public AuthResolver(AuthDiagnostics diagnostics, RunConfiguration config)
    {
        _diagnostics = diagnostics;
        _config = config;
    }

    public async Task<AuthResult> ResolveAsync(string[] scopes, CancellationToken cancellationToken)
    {
        var policy = new CredentialResolutionPolicy(_config);
        var start = DateTimeOffset.UtcNow;
        var retryCount = 0;

        foreach (var source in policy.OrderedSources)
        {
            var credential = CreateCredential(source, _config);
            if (credential is null)
            {
                continue;
            }

            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    foreach (var scope in scopes)
                    {
                        _ = await credential.GetTokenAsync(
                            new TokenRequestContext(new[] { scope }),
                            cancellationToken);
                    }

                    return new AuthResult
                    {
                        IsSuccess = true,
                        SelectedSource = source,
                        RetryCount = retryCount
                    };
                }
                catch (Exception ex)
                {
                    retryCount++;
                    var category = _diagnostics.Classify(ex);
                    var detail = _diagnostics.FormatException(ex);

                    if (category != AuthErrorCategory.Transient)
                    {
                        return new AuthResult
                        {
                            IsSuccess = false,
                            SelectedSource = source,
                            ErrorCategory = category,
                            ErrorMessage = "Authentication failed with a non-retryable error.",
                            ErrorDetail = detail,
                            RetryCount = retryCount
                        };
                    }

                    if (DateTimeOffset.UtcNow - start > RetryWindow || attempt == MaxAttempts)
                    {
                        return new AuthResult
                        {
                            IsSuccess = false,
                            SelectedSource = source,
                            ErrorCategory = AuthErrorCategory.Transient,
                            ErrorMessage = "Authentication failed after bounded retries.",
                            ErrorDetail = detail,
                            RetryCount = retryCount
                        };
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
        }

        return new AuthResult
        {
            IsSuccess = false,
            ErrorCategory = AuthErrorCategory.Fatal,
            ErrorMessage = "No valid credential sources available.",
            ErrorDetail = "No valid credential sources available.",
            RetryCount = retryCount
        };
    }

    private static TokenCredential? CreateCredential(CredentialSource source, RunConfiguration config)
    {
        return source switch
        {
            CredentialSource.ManagedIdentity => config.DisableManagedIdentity ? null : new ManagedIdentityCredential(),
            CredentialSource.WorkloadIdentity => config.DisableWorkloadIdentity ? null : new WorkloadIdentityCredential(),
            CredentialSource.LocalDeveloper => CreateLocalDeveloperCredential(config),
            _ => null
        };
    }

    private static TokenCredential? CreateLocalDeveloperCredential(RunConfiguration config)
    {
        var credentials = new List<TokenCredential>();

        if (!config.DisableAzureCli)
        {
            credentials.Add(new AzureCliCredential());
        }

        if (!config.DisableVisualStudio)
        {
            credentials.Add(new VisualStudioCredential());
        }

        if (!config.DisableVisualStudioCode)
        {
            credentials.Add(new VisualStudioCodeCredential());
        }

        return credentials.Count == 0 ? null : new ChainedTokenCredential(credentials.ToArray());
    }
}
