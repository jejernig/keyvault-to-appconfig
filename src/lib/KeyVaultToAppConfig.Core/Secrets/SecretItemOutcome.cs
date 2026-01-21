namespace KeyVaultToAppConfig.Core.Secrets;

public sealed class SecretItemOutcome
{
    public string Key { get; set; } = string.Empty;
    public string OriginalUri { get; set; } = string.Empty;
    public SecretHandlingMode Mode { get; set; }
    public bool GuardrailsSatisfied { get; set; }
    public bool AllowedKey { get; set; }
    public string? ResolvedVersion { get; set; }
    public bool IsValid { get; set; }
    public string? FailureReason { get; set; }
    public SecretHandlingOutcome Outcome { get; set; }
}
