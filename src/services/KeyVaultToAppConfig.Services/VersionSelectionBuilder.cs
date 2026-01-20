using System.Text.Json;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Enumeration;

namespace KeyVaultToAppConfig.Services;

public static class VersionSelectionBuilder
{
    public static VersionSelection Build(RunConfiguration config)
    {
        var mode = ParseMode(config.VersionMode);

        if (mode == VersionSelectionMode.Explicit && !string.IsNullOrWhiteSpace(config.VersionMapPath))
        {
            var map = LoadVersionMap(config.VersionMapPath);
            return new VersionSelection
            {
                Mode = mode,
                ExplicitVersions = map
            };
        }

        return new VersionSelection
        {
            Mode = mode,
            ExplicitVersions = null
        };
    }

    private static VersionSelectionMode ParseMode(string? mode)
    {
        return string.Equals(mode, "explicit", StringComparison.OrdinalIgnoreCase)
            ? VersionSelectionMode.Explicit
            : VersionSelectionMode.Latest;
    }

    private static IDictionary<string, string> LoadVersionMap(string path)
    {
        var json = File.ReadAllText(path);
        var map = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        return map ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
