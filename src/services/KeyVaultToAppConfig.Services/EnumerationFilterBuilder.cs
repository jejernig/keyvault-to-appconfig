using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Enumeration;

namespace KeyVaultToAppConfig.Services;

public static class EnumerationFilterBuilder
{
    public static EnumerationFilter Build(RunConfiguration config)
    {
        IDictionary<string, string>? tags = null;
        if (TagFilter.TryParse(config.OnlyTag, out var tag))
        {
            tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [tag.Key] = tag.Value
            };
        }

        return new EnumerationFilter
        {
            Prefix = config.IncludePrefix,
            Regex = config.ExcludeRegex,
            Tags = tags,
            EnabledOnly = config.EnabledOnly
        };
    }
}
