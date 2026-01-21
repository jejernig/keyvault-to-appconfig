using KeyVaultToAppConfig.Core.Writes;

namespace KeyVaultToAppConfig.Services.Writes;

public sealed class MetadataPolicy
{
    public IDictionary<string, string> ApplyManagedMetadata(
        IDictionary<string, string> existingTags,
        ManagedMetadata managedMetadata)
    {
        if (existingTags is null)
        {
            throw new ArgumentNullException(nameof(existingTags));
        }

        if (managedMetadata is null)
        {
            throw new ArgumentNullException(nameof(managedMetadata));
        }

        var merged = new Dictionary<string, string>(existingTags, StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(managedMetadata.Source))
        {
            merged["managedBy"] = managedMetadata.Source;
        }

        if (managedMetadata.Timestamp.HasValue)
        {
            merged["managedAt"] = managedMetadata.Timestamp.Value.ToString("O");
        }

        foreach (var entry in managedMetadata.AdditionalTags)
        {
            merged[entry.Key] = entry.Value;
        }

        return merged;
    }
}
