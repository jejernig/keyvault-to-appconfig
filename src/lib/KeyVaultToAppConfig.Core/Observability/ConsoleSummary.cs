using System.Text.Json.Serialization;
using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.Core.Observability;

public sealed class ConsoleSummary
{
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;

    [JsonPropertyName("totals")]
    public Totals Totals { get; set; } = new();

    [JsonPropertyName("emittedAt")]
    public DateTimeOffset EmittedAt { get; set; } = DateTimeOffset.UtcNow;
}
