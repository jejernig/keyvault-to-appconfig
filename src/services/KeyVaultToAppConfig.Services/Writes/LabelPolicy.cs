using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Core.Writes;

namespace KeyVaultToAppConfig.Services.Writes;

public sealed class LabelPolicy
{
    public string ResolveLabel(DesiredEntry desiredEntry, LabelContext labelContext)
    {
        if (desiredEntry is null)
        {
            throw new ArgumentNullException(nameof(desiredEntry));
        }

        if (labelContext is null)
        {
            throw new ArgumentNullException(nameof(labelContext));
        }

        if (!string.IsNullOrWhiteSpace(desiredEntry.Label))
        {
            return desiredEntry.Label;
        }

        if (!string.IsNullOrWhiteSpace(labelContext.EnvironmentLabel))
        {
            return labelContext.EnvironmentLabel;
        }

        return labelContext.UseEmptyLabelWhenMissing ? string.Empty : desiredEntry.Label;
    }
}
