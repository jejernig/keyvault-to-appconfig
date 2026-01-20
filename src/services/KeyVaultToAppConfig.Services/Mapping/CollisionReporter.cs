using KeyVaultToAppConfig.Core.Mapping;

namespace KeyVaultToAppConfig.Services.Mapping;

public sealed class CollisionReporter
{
    private readonly Dictionary<string, CollisionEntry> _entries =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(string normalizedKey, string existingSourceKey, string newSourceKey, CollisionPolicy policy)
    {
        if (!_entries.TryGetValue(normalizedKey, out var entry))
        {
            entry = new CollisionEntry
            {
                NormalizedKey = normalizedKey,
                AppliedPolicy = policy,
                SourceKeys = new List<string> { existingSourceKey, newSourceKey }
            };
            _entries[normalizedKey] = entry;
            return;
        }

        if (!entry.SourceKeys.Contains(newSourceKey, StringComparer.OrdinalIgnoreCase))
        {
            entry.SourceKeys.Add(newSourceKey);
        }

        entry.AppliedPolicy = policy;
    }

    public CollisionReport? BuildReport()
    {
        if (_entries.Count == 0)
        {
            return null;
        }

        return new CollisionReport
        {
            Entries = _entries.Values.ToList()
        };
    }
}
