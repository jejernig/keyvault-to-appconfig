namespace KeyVaultToAppConfig.Core.Secrets;

public sealed class SecretModeRequest
{
    public SecretHandlingMode Mode { get; set; } = SecretHandlingMode.Reference;
    public List<string> SecretUris { get; set; } = new();
    public CopyValueGuardrail Guardrail { get; set; } = new();
    public RedactionPolicy RedactionPolicy { get; set; } = new();
    public string? CorrelationId { get; set; }
}
