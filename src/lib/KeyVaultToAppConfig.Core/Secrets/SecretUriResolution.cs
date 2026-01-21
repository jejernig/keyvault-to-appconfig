namespace KeyVaultToAppConfig.Core.Secrets;

public sealed class SecretUriResolution
{
    public string OriginalUri { get; set; } = string.Empty;
    public string? ResolvedUri { get; set; }
    public string? SecretName { get; set; }
    public string? Version { get; set; }
    public bool IsValid { get; set; }
    public string? FailureReason { get; set; }
}
