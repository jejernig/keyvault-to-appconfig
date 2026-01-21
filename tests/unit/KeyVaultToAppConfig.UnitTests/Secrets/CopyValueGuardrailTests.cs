using KeyVaultToAppConfig.Core.Secrets;
using KeyVaultToAppConfig.Services.Secrets;

namespace KeyVaultToAppConfig.UnitTests.Secrets;

public sealed class CopyValueGuardrailTests
{
    [Fact]
    public void Evaluate_ReturnsSatisfiedWhenFlagsAndKeysPresent()
    {
        var evaluator = new CopyValueGuardrailEvaluator();
        var guardrail = new CopyValueGuardrail
        {
            FlagProvided = true,
            SecondaryConfirmation = CopyValueGuardrailEvaluator.RequiredConfirmationText,
            AllowedKeys = new List<string> { "alpha" }
        };

        var result = evaluator.Evaluate(guardrail);

        Assert.True(result.GuardrailsSatisfied);
        Assert.Contains("alpha", result.AllowedKeys);
    }

    [Fact]
    public void Evaluate_ReturnsUnsatisfiedWhenAllowedKeysMissing()
    {
        var evaluator = new CopyValueGuardrailEvaluator();
        var guardrail = new CopyValueGuardrail
        {
            FlagProvided = true,
            SecondaryConfirmation = CopyValueGuardrailEvaluator.RequiredConfirmationText
        };

        var result = evaluator.Evaluate(guardrail);

        Assert.False(result.GuardrailsSatisfied);
    }
}
