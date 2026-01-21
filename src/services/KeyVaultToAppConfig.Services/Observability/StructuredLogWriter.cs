using System.Text.Json;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Observability;

namespace KeyVaultToAppConfig.Services.Observability;

public sealed class StructuredLogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public void Write(LogEntry entry)
    {
        var sanitized = Sanitize(entry);
        var json = JsonSerializer.Serialize(sanitized, JsonOptions);
        Console.WriteLine(json);
    }

    private static LogEntry Sanitize(LogEntry entry)
    {
        var redactedData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in entry.Data)
        {
            redactedData[kvp.Key] = Redaction.Redact(kvp.Value);
        }

        return new LogEntry
        {
            Timestamp = entry.Timestamp,
            Level = entry.Level,
            Event = entry.Event,
            CorrelationId = entry.CorrelationId,
            Message = Redaction.Redact(entry.Message),
            Data = redactedData
        };
    }
}
