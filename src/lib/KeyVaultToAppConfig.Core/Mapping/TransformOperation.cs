namespace KeyVaultToAppConfig.Core.Mapping;

public sealed class TransformOperation
{
    public TransformType TransformType { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public enum TransformType
{
    Trim,
    Upper,
    Lower,
    Prefix,
    Suffix,
    Replace,
    RegexReplace,
    CaptureSubstitute
}
