using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Core.Planning;

namespace KeyVaultToAppConfig.Services.Planning;

public sealed class DesiredStateBuilder
{
    public DesiredState Build(IEnumerable<DesiredEntry> entries, string? generatedBy = null)
    {
        if (entries is null)
        {
            throw new ArgumentNullException(nameof(entries));
        }

        var orderedEntries = entries
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Label, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new DesiredState
        {
            Entries = orderedEntries,
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedBy = generatedBy
        };
    }

    public DesiredState BuildFromMapping(
        MappingRun mappingRun,
        IEnumerable<string>? environmentLabels,
        Func<string, string> valueResolver,
        Func<string, string?>? contentTypeResolver = null,
        string? generatedBy = null)
    {
        if (mappingRun is null)
        {
            throw new ArgumentNullException(nameof(mappingRun));
        }

        if (valueResolver is null)
        {
            throw new ArgumentNullException(nameof(valueResolver));
        }

        var labels = NormalizeLabels(environmentLabels);
        if (labels.Count == 0)
        {
            labels.Add(string.Empty);
        }

        var entries = new List<DesiredEntry>();
        var orderedMappings = mappingRun.NormalizedKeys
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        foreach (var mapping in orderedMappings)
        {
            foreach (var label in labels)
            {
                entries.Add(new DesiredEntry
                {
                    Key = mapping.Key,
                    Label = label,
                    Value = valueResolver(mapping.Value),
                    ContentType = contentTypeResolver?.Invoke(mapping.Value),
                    SourceId = mapping.Value
                });
            }
        }

        return Build(entries, generatedBy);
    }

    private static List<string> NormalizeLabels(IEnumerable<string>? labels)
    {
        return labels?
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select(label => label.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(label => label, StringComparer.OrdinalIgnoreCase)
            .ToList()
            ?? new List<string>();
    }
}
