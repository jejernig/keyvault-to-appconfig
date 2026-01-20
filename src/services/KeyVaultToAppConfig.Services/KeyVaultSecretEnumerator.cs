using System.Text.RegularExpressions;
using Azure;
using Azure.Security.KeyVault.Secrets;
using KeyVaultToAppConfig.Core.Enumeration;

namespace KeyVaultToAppConfig.Services;

public sealed class KeyVaultSecretEnumerator
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(30);

    private readonly SecretClient _client;

    public KeyVaultSecretEnumerator(SecretClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<SecretDescriptor>> EnumerateAsync(
        EnumerationFilter filter,
        VersionSelection versionSelection,
        int? pageSize,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        var regex = string.IsNullOrWhiteSpace(filter.Regex)
            ? null
            : new Regex(filter.Regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        var results = new List<SecretDescriptor>();
        var nextToken = continuationToken;
        var retryAttempts = 0;

        while (true)
        {
            var pageEnumerator = _client
                .GetPropertiesOfSecretsAsync(cancellationToken)
                .AsPages(nextToken, pageSize)
                .GetAsyncEnumerator(cancellationToken);

            try
            {
                if (!await pageEnumerator.MoveNextAsync())
                {
                    break;
                }

                var page = pageEnumerator.Current;
                nextToken = page.ContinuationToken;

                foreach (var properties in page.Values)
                {
                    if (!MatchesPrefix(properties.Name, filter.Prefix))
                    {
                        continue;
                    }

                    if (filter.EnabledOnly && properties.Enabled is false)
                    {
                        continue;
                    }

                    if (regex is not null && !regex.IsMatch(properties.Name))
                    {
                        continue;
                    }

                    if (!MatchesTags(properties.Tags, filter.Tags))
                    {
                        continue;
                    }

                    var selected = await SelectVersionAsync(properties, versionSelection, cancellationToken);
                    if (selected is null)
                    {
                        continue;
                    }

                    results.Add(ToDescriptor(selected));
                }

                retryAttempts = 0;
                if (string.IsNullOrWhiteSpace(nextToken))
                {
                    break;
                }
            }
            catch (RequestFailedException ex) when (IsRetryable(ex) && retryAttempts < MaxRetries)
            {
                retryAttempts++;
                var delay = CalculateDelay(retryAttempts);
                await Task.Delay(delay, cancellationToken);
                continue;
            }
            finally
            {
                await pageEnumerator.DisposeAsync();
            }
        }

        return SecretOrdering.Order(results);
    }

    private async Task<SecretProperties?> SelectVersionAsync(
        SecretProperties properties,
        VersionSelection selection,
        CancellationToken cancellationToken)
    {
        if (selection.Mode == VersionSelectionMode.Latest)
        {
            return properties;
        }

        if (selection.ExplicitVersions is null)
        {
            return null;
        }

        if (!selection.ExplicitVersions.TryGetValue(properties.Name, out var version))
        {
            return null;
        }

        return await FindExplicitVersionAsync(properties.Name, version, cancellationToken);
    }

    private static bool MatchesPrefix(string name, string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return true;
        }

        return name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesTags(IDictionary<string, string> tags, IDictionary<string, string>? required)
    {
        if (required is null || required.Count == 0)
        {
            return true;
        }

        foreach (var entry in required)
        {
            if (!tags.TryGetValue(entry.Key, out var value))
            {
                return false;
            }

            if (!string.Equals(value, entry.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static SecretDescriptor ToDescriptor(SecretProperties properties)
    {
        return new SecretDescriptor
        {
            Name = properties.Name,
            Version = properties.Version ?? string.Empty,
            IsEnabled = properties.Enabled ?? true,
            Tags = properties.Tags ?? new Dictionary<string, string>(),
            LastUpdated = properties.UpdatedOn ?? DateTimeOffset.MinValue
        };
    }

    private static bool IsRetryable(RequestFailedException ex)
    {
        return ex.Status is 429 or 503;
    }

    private static TimeSpan CalculateDelay(int attempt)
    {
        var delay = TimeSpan.FromSeconds(BaseDelay.TotalSeconds * Math.Pow(2, attempt - 1));
        return delay > MaxDelay ? MaxDelay : delay;
    }

    private static async Task<T> ExecuteWithRetriesAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxRetries + 1; attempt++)
        {
            try
            {
                return await action(cancellationToken);
            }
            catch (RequestFailedException ex) when (IsRetryable(ex) && attempt <= MaxRetries)
            {
                var delay = CalculateDelay(attempt);
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw new InvalidOperationException("Retry loop exited unexpectedly.");
    }

    private async Task<SecretProperties?> FindExplicitVersionAsync(
        string name,
        string version,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxRetries + 1; attempt++)
        {
            try
            {
                await foreach (var props in _client.GetPropertiesOfSecretVersionsAsync(name, cancellationToken))
                {
                    if (string.Equals(props.Version, version, StringComparison.OrdinalIgnoreCase))
                    {
                        return props;
                    }
                }

                return null;
            }
            catch (RequestFailedException ex) when (IsRetryable(ex) && attempt <= MaxRetries)
            {
                var delay = CalculateDelay(attempt);
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw new InvalidOperationException("Retry loop exited unexpectedly.");
    }
}
