namespace KeyVaultToAppConfig.Services.Observability;

public sealed class CorrelationIdProvider
{
    public string GetOrCreate(string? provided)
    {
        return string.IsNullOrWhiteSpace(provided)
            ? Guid.NewGuid().ToString("n")
            : provided.Trim();
    }
}
