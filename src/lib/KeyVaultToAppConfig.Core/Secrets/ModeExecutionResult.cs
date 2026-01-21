namespace KeyVaultToAppConfig.Core.Secrets;

public sealed class ModeExecutionResult
{
    public string CorrelationId { get; set; } = string.Empty;
    public SecretHandlingMode Mode { get; set; }
    public bool GuardrailsSatisfied { get; set; }
    public bool RedactionApplied { get; set; }
    public bool AllowedKeyEnforcementApplied { get; set; }
    public List<SecretItemOutcome> Items { get; set; } = new();
    public List<string> Messages { get; set; } = new();
}
