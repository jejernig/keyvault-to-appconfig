using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.Services.Mapping;

public static class MappingLogging
{
    public static string RedactMessage(string message)
    {
        return Redaction.Redact(message);
    }

    public static string RedactKey(string key)
    {
        return Redaction.Redact(key);
    }
}
