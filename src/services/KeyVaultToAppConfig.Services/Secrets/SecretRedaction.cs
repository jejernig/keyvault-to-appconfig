using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Secrets;

namespace KeyVaultToAppConfig.Services.Secrets;

public sealed class SecretRedaction
{
    public string RedactValue(string value, RedactionPolicy policy)
    {
        if (!policy.RedactValues)
        {
            return value;
        }

        return Redaction.Redact(value);
    }

    public string RedactName(string name, RedactionPolicy policy)
    {
        if (!policy.RedactNames)
        {
            return name;
        }

        return "[REDACTED]";
    }
}
