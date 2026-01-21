using KeyVaultToAppConfig.Core.Secrets;

namespace KeyVaultToAppConfig.Services.Secrets;

public sealed class SecretUriResolver
{
    private const string LatestVersionMarker = "latest";

    public SecretUriResolution Resolve(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return new SecretUriResolution
            {
                OriginalUri = uri ?? string.Empty,
                IsValid = false,
                FailureReason = "Secret URI is required."
            };
        }

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
        {
            return new SecretUriResolution
            {
                OriginalUri = uri,
                IsValid = false,
                FailureReason = "Secret URI is not a valid absolute URI."
            };
        }

        if (!string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return new SecretUriResolution
            {
                OriginalUri = uri,
                IsValid = false,
                FailureReason = "Secret URI must use https."
            };
        }

        if (!parsed.Host.EndsWith(".vault.azure.net", StringComparison.OrdinalIgnoreCase))
        {
            return new SecretUriResolution
            {
                OriginalUri = uri,
                IsValid = false,
                FailureReason = "Secret URI host must be a Key Vault endpoint."
            };
        }

        var segments = parsed.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2 || !string.Equals(segments[0], "secrets", StringComparison.OrdinalIgnoreCase))
        {
            return new SecretUriResolution
            {
                OriginalUri = uri,
                IsValid = false,
                FailureReason = "Secret URI must target a secret resource."
            };
        }

        var secretName = segments[1];
        var version = segments.Length >= 3 ? segments[2] : null;
        var resolvedVersion = string.IsNullOrWhiteSpace(version) ? LatestVersionMarker : version;
        var resolvedUri = string.IsNullOrWhiteSpace(version)
            ? $"{uri.TrimEnd('/')}/{resolvedVersion}"
            : uri;

        return new SecretUriResolution
        {
            OriginalUri = uri,
            ResolvedUri = resolvedUri,
            SecretName = secretName,
            Version = resolvedVersion,
            IsValid = true
        };
    }
}
