namespace KeyVaultToAppConfig.Core.Secrets;

public sealed class CopyValueGuardrail
{
    public bool FlagProvided { get; set; }
    public string? SecondaryConfirmation { get; set; }
    public List<string> AllowedKeys { get; set; } = new();
}
