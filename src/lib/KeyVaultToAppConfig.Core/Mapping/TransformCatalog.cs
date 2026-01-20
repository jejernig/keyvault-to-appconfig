namespace KeyVaultToAppConfig.Core.Mapping;

public static class TransformCatalog
{
    private static readonly Dictionary<string, TransformType> Supported =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["trim"] = TransformType.Trim,
            ["upper"] = TransformType.Upper,
            ["lower"] = TransformType.Lower,
            ["prefix"] = TransformType.Prefix,
            ["suffix"] = TransformType.Suffix,
            ["replace"] = TransformType.Replace,
            ["regex-replace"] = TransformType.RegexReplace,
            ["capture-substitute"] = TransformType.CaptureSubstitute
        };

    public static IReadOnlyCollection<string> Names => Supported.Keys;

    public static bool TryParse(string name, out TransformType transformType)
    {
        return Supported.TryGetValue(name, out transformType);
    }
}
