using KeyVaultToAppConfig.Core.Planning;

namespace KeyVaultToAppConfig.Services.Planning;

public sealed class PlanningEngine : IPlanningEngine
{
    private readonly DiffClassifier _classifier = new();

    public Task<PlanOutput> BuildPlanAsync(
        DesiredState desiredState,
        ExistingStateSnapshot existingState,
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

        var orderedDesired = desiredState.Entries
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Label, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var conflicts = BuildConflicts(orderedDesired);
        var conflictKeys = new HashSet<string>(conflicts.Select(conflict => BuildKey(conflict.Key, conflict.Label)),
            StringComparer.OrdinalIgnoreCase);

        var existingLookup = BuildExistingLookup(existingState.Entries);
        var diffItems = new List<DiffItem>();

        foreach (var desiredEntry in orderedDesired)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (conflictKeys.Contains(BuildKey(desiredEntry.Key, desiredEntry.Label)))
            {
                continue;
            }

            existingLookup.TryGetValue(BuildKey(desiredEntry.Key, desiredEntry.Label), out var existingEntry);
            var diffItem = _classifier.Classify(desiredEntry, existingEntry);
            diffItems.Add(diffItem);
        }

        var output = new PlanOutput
        {
            DiffItems = diffItems,
            Conflicts = conflicts,
            GeneratedAt = DateTimeOffset.UtcNow,
            Totals = new PlanTotals
            {
                CreateCount = diffItems.Count(item => item.Classification == DiffClassification.Create),
                UpdateCount = diffItems.Count(item => item.Classification == DiffClassification.Update),
                UnchangedCount = diffItems.Count(item => item.Classification == DiffClassification.Unchanged),
                ConflictCount = conflicts.Count
            }
        };

        return Task.FromResult(output);
    }

    private static Dictionary<string, ExistingEntry> BuildExistingLookup(IEnumerable<ExistingEntry> entries)
    {
        var orderedExisting = entries
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Label, StringComparer.OrdinalIgnoreCase);

        var lookup = new Dictionary<string, ExistingEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in orderedExisting)
        {
            var key = BuildKey(entry.Key, entry.Label);
            if (!lookup.ContainsKey(key))
            {
                lookup[key] = entry;
            }
        }

        return lookup;
    }

    private static List<ConflictRecord> BuildConflicts(IEnumerable<DesiredEntry> desiredEntries)
    {
        return desiredEntries
            .GroupBy(entry => BuildKey(entry.Key, entry.Label), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ConflictRecord
            {
                Key = group.First().Key,
                Label = group.First().Label,
                ConflictingValues = group.Select(entry => entry.Value)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList(),
                ResolutionStatus = "Unresolved"
            })
            .ToList();
    }

    private static string BuildKey(string key, string label) => $"{key}::{label}";
}
