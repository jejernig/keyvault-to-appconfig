namespace KeyVaultToAppConfig.Core;

public static class DeterministicOrdering
{
    public static IEnumerable<ChangeSummary> OrderChanges(IEnumerable<ChangeSummary> changes)
    {
        return changes
            .OrderBy(change => change.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(change => change.Label ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(change => change.Action, StringComparer.OrdinalIgnoreCase);
    }
}
