using KeyVaultToAppConfig.Core.Secrets;

namespace KeyVaultToAppConfig.Services.Secrets;

public sealed class CopyValueGuardrailEvaluator
{
    public const string RequiredConfirmationText = "I UNDERSTAND";

    public CopyValueGuardrailResult Evaluate(CopyValueGuardrail guardrail)
    {
        var allowedKeys = new HashSet<string>(
            guardrail.AllowedKeys ?? new List<string>(),
            StringComparer.OrdinalIgnoreCase);

        var hasFlag = guardrail.FlagProvided;
        var hasConfirmation = string.Equals(
            guardrail.SecondaryConfirmation?.Trim(),
            RequiredConfirmationText,
            StringComparison.OrdinalIgnoreCase);

        var satisfied = hasFlag && hasConfirmation && allowedKeys.Count > 0;

        return new CopyValueGuardrailResult
        {
            GuardrailsSatisfied = satisfied,
            AllowedKeys = allowedKeys
        };
    }
}
