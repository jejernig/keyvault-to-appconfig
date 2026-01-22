using System.Text.RegularExpressions;
using Azure;
using Azure.Identity;
using KeyVaultToAppConfig.Core.Auth;

namespace KeyVaultToAppConfig.Services;

public sealed class AuthDiagnostics
{
    private static readonly Regex SecretPattern = new("(?i)(clientsecret|secret=|password=|apikey=)", RegexOptions.Compiled);

    public IReadOnlyList<string> ValidateNoStaticSecrets(IEnumerable<string?> inputs)
    {
        var violations = new List<string>();

        foreach (var input in inputs)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (SecretPattern.IsMatch(input))
            {
                violations.Add("Static secret values are not permitted in configuration inputs.");
                break;
            }
        }

        return violations;
    }

    public AuthErrorCategory Classify(Exception exception)
    {
        if (exception is AuthenticationFailedException authException
            && authException.Message.Contains("authorization", StringComparison.OrdinalIgnoreCase))
        {
            return AuthErrorCategory.Rbac;
        }

        if (exception is AuthenticationFailedException)
        {
            return AuthErrorCategory.Fatal;
        }

        return AuthErrorCategory.Transient;
    }

    public string FormatException(Exception exception)
    {
        var message = exception.Message;
        if (SecretPattern.IsMatch(message))
        {
            message = SecretPattern.Replace(message, "redacted=");
        }

        if (exception is AuthenticationFailedException authException
            && authException.InnerException is RequestFailedException requestFailed)
        {
            return $"{exception.GetType().Name}: {message} (Status: {requestFailed.Status}, Code: {requestFailed.ErrorCode})";
        }

        return $"{exception.GetType().Name}: {message}";
    }
}
