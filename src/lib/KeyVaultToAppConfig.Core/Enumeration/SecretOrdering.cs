namespace KeyVaultToAppConfig.Core.Enumeration;

public static class SecretOrdering
{
    public static IReadOnlyList<SecretDescriptor> Order(IEnumerable<SecretDescriptor> items)
    {
        return items
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Version, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
