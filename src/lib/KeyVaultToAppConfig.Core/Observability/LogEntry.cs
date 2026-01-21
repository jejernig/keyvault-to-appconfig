using System.Text.Json.Serialization;

namespace KeyVaultToAppConfig.Core.Observability;

public sealed class LogEntry
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("level")]
    public string Level { get; set; } = "info";

    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public IDictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
}
