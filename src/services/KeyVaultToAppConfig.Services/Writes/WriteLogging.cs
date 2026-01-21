using System.Text.Json;
using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.Services.Writes;

public static class WriteLogging
{
    public static string CreateCorrelationId()
    {
        return Guid.NewGuid().ToString("N");
    }

    public static string BuildStructuredLog(
        string correlationId,
        string eventName,
        IReadOnlyDictionary<string, string?>? fields = null)
    {
        var sanitized = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (fields is not null)
        {
            foreach (var entry in fields)
            {
                sanitized[entry.Key] = Redaction.Redact(entry.Value);
            }
        }

        var payload = new Dictionary<string, object?>
        {
            ["timestamp"] = DateTimeOffset.UtcNow,
            ["correlationId"] = correlationId,
            ["event"] = eventName,
            ["fields"] = sanitized
        };

        return JsonSerializer.Serialize(payload);
    }
}
