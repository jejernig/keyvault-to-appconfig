namespace KeyVaultToAppConfig.Core.Mapping;

public sealed class SpecValidationError
{
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
}
