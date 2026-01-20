namespace KeyVaultToAppConfig.Core.Enumeration;

public sealed class EnumerationPage
{
    public IReadOnlyList<SecretDescriptor> Items { get; init; } = Array.Empty<SecretDescriptor>();
    public string? ContinuationToken { get; init; }
}
