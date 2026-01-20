using System.Text.RegularExpressions;

namespace KeyVaultToAppConfig.Core;

public static class Redaction
{
    private static readonly Regex ValuePattern = new(
        "(?i)((value|clientsecret|secret|password|apikey)\\s*[:=]\\s*)(\\S+)",
        RegexOptions.Compiled);

    public static string Redact(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input ?? string.Empty;
        }

        return ValuePattern.Replace(input, "$1[REDACTED]");
    }
}
