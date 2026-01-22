using KeyVaultToAppConfig.Core.Errors;

namespace KeyVaultToAppConfig.Services.Errors;

public sealed class ErrorClassifier
{
    public ErrorClassification Classify(string scope, Exception exception)
    {
        if (string.Equals(scope, "secret", StringComparison.OrdinalIgnoreCase))
        {
            return ErrorClassification.Recoverable;
        }

        return ErrorClassification.Fatal;
    }
}
