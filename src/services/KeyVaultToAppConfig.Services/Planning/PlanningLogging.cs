using System.Text.Json;
using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.Services.Planning;

public static class PlanningLogging
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

    public static string RedactMessage(string message)
    {
        return Redaction.Redact(message);
    }

    public static string RedactKey(string key)
    {
        return Redaction.Redact(key);
    }

    public static string RedactValue(string value)
    {
        return Redaction.Redact(value);
    }
}
