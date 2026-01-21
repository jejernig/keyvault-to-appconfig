namespace KeyVaultToAppConfig.Services.Secrets;

public sealed class CopyValueGuardrailResult
{
    public bool GuardrailsSatisfied { get; set; }
    public HashSet<string> AllowedKeys { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
