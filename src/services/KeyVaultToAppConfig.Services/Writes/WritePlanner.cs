using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Core.Writes;

namespace KeyVaultToAppConfig.Services.Writes;

public sealed class WritePlanner : IWritePlanner
{
    private readonly LabelPolicy _labelPolicy = new();

    public Task<WritePlan> BuildPlanAsync(
        DesiredState desiredState,
        ExistingStateSnapshot existingState,
        LabelContext labelContext,
        ManagedMetadata managedMetadata,
        CancellationToken cancellationToken)
    {
        if (desiredState is null)
        {
            throw new ArgumentNullException(nameof(desiredState));
        }

        if (existingState is null)
        {
            throw new ArgumentNullException(nameof(existingState));
        }

        if (labelContext is null)
        {
            throw new ArgumentNullException(nameof(labelContext));
        }

        if (managedMetadata is null)
        {
            throw new ArgumentNullException(nameof(managedMetadata));
        }

        var existingLookup = existingState.Entries
            .GroupBy(entry => BuildKey(entry.Key, entry.Label), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var orderedDesired = desiredState.Entries
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Label, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var actions = new List<WriteAction>();
        var duplicates = orderedDesired
            .GroupBy(entry => BuildKey(entry.Key, _labelPolicy.ResolveLabel(entry, labelContext)),
                StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var desiredEntry in orderedDesired)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var resolvedLabel = _labelPolicy.ResolveLabel(desiredEntry, labelContext);
            var key = BuildKey(desiredEntry.Key, resolvedLabel);

            if (duplicates.Contains(key))
            {
                actions.Add(new WriteAction
                {
                    Key = desiredEntry.Key,
                    Label = resolvedLabel,
                    ActionType = WriteActionType.Skip,
                    DesiredValue = desiredEntry.Value,
                    DesiredContentType = desiredEntry.ContentType,
                    Reason = "Conflict: multiple desired entries for key/label"
                });
                continue;
            }

            if (!existingLookup.TryGetValue(key, out var existing))
            {
                actions.Add(new WriteAction
                {
                    Key = desiredEntry.Key,
                    Label = resolvedLabel,
                    ActionType = WriteActionType.Create,
                    DesiredValue = desiredEntry.Value,
                    DesiredContentType = desiredEntry.ContentType,
                    Reason = "Missing in existing state"
                });
                continue;
            }

            var valueMatches = string.Equals(desiredEntry.Value, existing.Value, StringComparison.Ordinal);
            var contentMatches = string.Equals(
                desiredEntry.ContentType ?? string.Empty,
                existing.ContentType ?? string.Empty,
                StringComparison.Ordinal);

            if (valueMatches && contentMatches)
            {
                actions.Add(new WriteAction
                {
                    Key = desiredEntry.Key,
                    Label = resolvedLabel,
                    ActionType = WriteActionType.Skip,
                    DesiredValue = desiredEntry.Value,
                    DesiredContentType = desiredEntry.ContentType,
                    Reason = "Unchanged"
                });
                continue;
            }

            actions.Add(new WriteAction
            {
                Key = desiredEntry.Key,
                Label = resolvedLabel,
                ActionType = WriteActionType.Update,
                DesiredValue = desiredEntry.Value,
                DesiredContentType = desiredEntry.ContentType,
                Reason = valueMatches ? "Content type differs" : "Value differs"
            });
        }

        var plan = new WritePlan
        {
            Actions = actions,
            LabelContext = labelContext,
            ManagedMetadata = managedMetadata
        };

        return Task.FromResult(plan);
    }

    private static string BuildKey(string key, string label) => $"{key}::{label}";
}
