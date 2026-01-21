namespace KeyVaultToAppConfig.Core.Secrets;

public sealed class RedactionPolicy
{
    public bool RedactValues { get; set; } = true;
    public bool RedactNames { get; set; }
    public List<string> SensitivePatterns { get; set; } = new();
    public List<string> RedactedFields { get; set; } = new();
}
