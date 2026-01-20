namespace KeyVaultToAppConfig.Core.Enumeration;

public static class TagFilter
{
    public static bool TryParse(string? raw, out KeyValuePair<string, string> tag)
    {
        tag = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var parts = raw.Split('=', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            return false;
        }

        tag = new KeyValuePair<string, string>(parts[0], parts[1]);
        return true;
    }

    public static bool Matches(IDictionary<string, string>? tags, KeyValuePair<string, string> required)
    {
        if (tags is null)
        {
            return false;
        }

        return tags.TryGetValue(required.Key, out var value)
            && string.Equals(value, required.Value, StringComparison.OrdinalIgnoreCase);
    }
}
