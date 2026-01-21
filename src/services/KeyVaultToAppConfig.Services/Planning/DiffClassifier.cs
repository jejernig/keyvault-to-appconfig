using KeyVaultToAppConfig.Core.Planning;

namespace KeyVaultToAppConfig.Services.Planning;

public sealed class DiffClassifier
{
    public DiffItem Classify(DesiredEntry desiredEntry, ExistingEntry? existingEntry)
    {
        if (desiredEntry is null)
        {
            throw new ArgumentNullException(nameof(desiredEntry));
        }

        if (existingEntry is null)
        {
            return new DiffItem
            {
                Key = desiredEntry.Key,
                Label = desiredEntry.Label,
                Classification = DiffClassification.Create,
                Reason = "Missing in existing state",
                DesiredValue = desiredEntry.Value,
                DesiredContentType = desiredEntry.ContentType
            };
        }

        var valueMatches = string.Equals(desiredEntry.Value, existingEntry.Value, StringComparison.Ordinal);
        var contentTypeMatches = string.Equals(desiredEntry.ContentType ?? string.Empty,
            existingEntry.ContentType ?? string.Empty, StringComparison.Ordinal);

        if (valueMatches && contentTypeMatches)
        {
            return new DiffItem
            {
                Key = desiredEntry.Key,
                Label = desiredEntry.Label,
                Classification = DiffClassification.Unchanged,
                Reason = "Matches existing state",
                DesiredValue = desiredEntry.Value,
                ExistingValue = existingEntry.Value,
                DesiredContentType = desiredEntry.ContentType,
                ExistingContentType = existingEntry.ContentType
            };
        }

        var reason = valueMatches
            ? "Content type differs"
            : contentTypeMatches
                ? "Value differs"
                : "Value and content type differ";

        return new DiffItem
        {
            Key = desiredEntry.Key,
            Label = desiredEntry.Label,
            Classification = DiffClassification.Update,
            Reason = reason,
            DesiredValue = desiredEntry.Value,
            ExistingValue = existingEntry.Value,
            DesiredContentType = desiredEntry.ContentType,
            ExistingContentType = existingEntry.ContentType
        };
    }
}
