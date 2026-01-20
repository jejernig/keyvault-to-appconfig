using Azure.Core;
using Azure.Identity;
using KeyVaultToAppConfig.Core.Auth;

namespace KeyVaultToAppConfig.Services;

public sealed class AuthResolver
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan RetryWindow = TimeSpan.FromSeconds(10);

    private readonly AuthDiagnostics _diagnostics;

    public AuthResolver(AuthDiagnostics diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public async Task<AuthResult> ResolveAsync(string[] scopes, CancellationToken cancellationToken)
    {
        var policy = new CredentialResolutionPolicy();
        var start = DateTimeOffset.UtcNow;
        var retryCount = 0;

        foreach (var source in policy.OrderedSources)
        {
            var credential = CreateCredential(source);
            if (credential is null)
            {
                continue;
            }

            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    _ = await credential.GetTokenAsync(new TokenRequestContext(scopes), cancellationToken);

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

                    if (category != AuthErrorCategory.Transient)
                    {
                        return new AuthResult
                        {
                            IsSuccess = false,
                            SelectedSource = source,
                            ErrorCategory = category,
                            ErrorMessage = "Authentication failed with a non-retryable error.",
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
            RetryCount = retryCount
        };
    }

    private static TokenCredential? CreateCredential(CredentialSource source)
    {
        return source switch
        {
            CredentialSource.ManagedIdentity => new ManagedIdentityCredential(),
            CredentialSource.WorkloadIdentity => new WorkloadIdentityCredential(),
            CredentialSource.LocalDeveloper => new ChainedTokenCredential(
                new AzureCliCredential(),
                new VisualStudioCredential(),
                new VisualStudioCodeCredential()),
            _ => null
        };
    }
}
