using KeyVaultToAppConfig.Core.Planning;

namespace KeyVaultToAppConfig.Cli.Commands;

internal static class PlanningCommandHelpers
{
    public static List<string> NormalizeLabels(string[]? labels)
    {
        return labels?
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select(label => label.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(label => label, StringComparer.OrdinalIgnoreCase)
            .ToList()
            ?? new List<string>();
    }

    public static ExistingStateSnapshot ApplyScope(
        ExistingStateSnapshot snapshot,
        string? keyPrefix,
        IReadOnlyList<string> labels,
        int? pageSize,
        string? continuationToken,
        out string? error)
    {
        error = null;
        var entries = snapshot.Entries.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyPrefix))
        {
            entries = entries.Where(entry =>
                entry.Key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase));
        }

        if (labels.Count > 0)
        {
            entries = entries.Where(entry =>
                labels.Contains(entry.Label, StringComparer.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(continuationToken))
        {
            if (!int.TryParse(continuationToken, out var offset) || offset < 0)
            {
                error = "continuation-token: Must be a non-negative integer for file-based inputs.";
                return snapshot;
            }

            entries = entries.Skip(offset);
        }

        if (pageSize.HasValue)
        {
            entries = entries.Take(pageSize.Value);
        }

        return new ExistingStateSnapshot
        {
            Entries = entries.ToList(),
            RetrievedAt = snapshot.RetrievedAt,
            KeyPrefix = keyPrefix,
            Labels = labels.ToArray(),
            PageSize = pageSize,
            ContinuationToken = continuationToken
        };
    }
}
