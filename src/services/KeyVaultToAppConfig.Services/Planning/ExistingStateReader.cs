using Azure.Data.AppConfiguration;
using KeyVaultToAppConfig.Core.Planning;

namespace KeyVaultToAppConfig.Services.Planning;

public sealed class ExistingStateReader : IExistingStateReader
{
    private readonly ConfigurationClient _client;

    public ExistingStateReader(ConfigurationClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<ExistingStateSnapshot> ReadAsync(
        string? keyPrefix,
        IReadOnlyList<string> labels,
        int? pageSize,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        if (labels is null)
        {
            throw new ArgumentNullException(nameof(labels));
        }

        var keyFilter = string.IsNullOrWhiteSpace(keyPrefix) ? null : $"{keyPrefix}*";
        var labelFilters = labels.Count == 0 ? new string?[] { null } : labels.Cast<string?>();
        var entries = new List<ExistingEntry>();
        var finalContinuationToken = labels.Count <= 1 ? continuationToken : null;

        foreach (var labelFilter in labelFilters)
        {
            var selector = new SettingSelector
            {
                KeyFilter = keyFilter,
                LabelFilter = labelFilter
            };

            var pages = _client.GetConfigurationSettingsAsync(selector, cancellationToken)
                .AsPages(finalContinuationToken, pageSize);

            await foreach (var page in pages.WithCancellation(cancellationToken))
            {
                foreach (var setting in page.Values)
                {
                    entries.Add(new ExistingEntry
                    {
                        Key = setting.Key,
                        Label = setting.Label ?? string.Empty,
                        Value = setting.Value ?? string.Empty,
                        ContentType = setting.ContentType,
                        LastModified = setting.LastModified
                    });
                }

                finalContinuationToken = page.ContinuationToken;
            }
        }

        return new ExistingStateSnapshot
        {
            Entries = entries,
            RetrievedAt = DateTimeOffset.UtcNow,
            KeyPrefix = keyPrefix,
            Labels = labels.ToArray(),
            PageSize = pageSize,
            ContinuationToken = finalContinuationToken
        };
    }
}
