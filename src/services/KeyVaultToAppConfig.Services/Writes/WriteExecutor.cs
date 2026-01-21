using Azure.Data.AppConfiguration;
using KeyVaultToAppConfig.Core.Writes;

namespace KeyVaultToAppConfig.Services.Writes;

public sealed class WriteExecutor : IWriteExecutor
{
    private readonly ConfigurationClient _client;
    private readonly MetadataPolicy _metadataPolicy = new();
    private readonly RetryPolicyExecutor _retryExecutor = new();

    public WriteExecutor(ConfigurationClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<WriteReport> ExecuteAsync(
        WritePlan plan,
        WriteOptions options,
        CancellationToken cancellationToken)
    {
        if (plan is null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var report = new WriteReport
        {
            CorrelationId = WriteLogging.CreateCorrelationId(),
            StartedAt = DateTimeOffset.UtcNow
        };

        var results = new List<WriteResult>();
        var semaphore = new SemaphoreSlim(Math.Max(1, options.MaxParallelism));
        var tasks = plan.Actions.Select(async action =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await ExecuteActionAsync(action, plan.ManagedMetadata, options.RetryPolicy, cancellationToken);
                lock (results)
                {
                    results.Add(result);
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        if (options.RollbackPlan?.Enabled == true && plan.RollbackPlan?.Actions.Count > 0)
        {
            var rollbackHandler = new RollbackHandler(_client);
            var rollbackLookup = plan.RollbackPlan.Actions.ToDictionary(
                action => BuildKey(action.Key, action.Label),
                action => action,
                StringComparer.OrdinalIgnoreCase);

            foreach (var result in results.Where(result => result.Status == WriteStatus.Failed))
            {
                if (rollbackLookup.TryGetValue(BuildKey(result.Key, result.Label), out var rollbackAction))
                {
                    await rollbackHandler.TryRollbackAsync(result, rollbackAction, cancellationToken);
                }
            }
        }

        report.Results = results
            .OrderBy(result => result.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(result => result.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
        report.CompletedAt = DateTimeOffset.UtcNow;
        report.Totals = BuildTotals(report.Results, plan.Actions);

        return report;
    }

    private async Task<WriteResult> ExecuteActionAsync(
        WriteAction action,
        ManagedMetadata? managedMetadata,
        RetryPolicy retryPolicy,
        CancellationToken cancellationToken)
    {
        if (action.ActionType == WriteActionType.Skip)
        {
            return new WriteResult
            {
                Key = action.Key,
                Label = action.Label,
                Status = WriteStatus.Skipped,
                Attempts = 0
            };
        }

        return await _retryExecutor.ExecuteAsync(
            action,
            retryPolicy,
            async token =>
            {
                var setting = new ConfigurationSetting(action.Key, action.DesiredValue ?? string.Empty)
                {
                    Label = action.Label,
                    ContentType = action.DesiredContentType
                };

                var tags = await ReadExistingTagsAsync(action, token);
                if (managedMetadata is not null)
                {
                    var merged = _metadataPolicy.ApplyManagedMetadata(tags, managedMetadata);
                    foreach (var entry in merged)
                    {
                        setting.Tags[entry.Key] = entry.Value;
                    }
                }

                await _client.SetConfigurationSettingAsync(setting, false, token);
            },
            cancellationToken);
    }

    private async Task<IDictionary<string, string>> ReadExistingTagsAsync(
        WriteAction action,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _client.GetConfigurationSettingAsync(
                action.Key,
                action.Label,
                cancellationToken);
            return existing?.Value?.Tags
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static WriteTotals BuildTotals(IEnumerable<WriteResult> results, IEnumerable<WriteAction> actions)
    {
        var totals = new WriteTotals();
        foreach (var action in actions)
        {
            totals.CreateCount += action.ActionType == WriteActionType.Create ? 1 : 0;
            totals.UpdateCount += action.ActionType == WriteActionType.Update ? 1 : 0;
            totals.SkipCount += action.ActionType == WriteActionType.Skip ? 1 : 0;
        }

        totals.FailedCount = results.Count(result =>
            result.Status == WriteStatus.Failed);

        return totals;
    }

    private static string BuildKey(string key, string label) => $"{key}::{label}";
}
