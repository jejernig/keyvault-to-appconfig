using System.Text.Json;
using System.Linq;
using KeyVaultToAppConfig.Core.Secrets;

namespace KeyVaultToAppConfig.Services.Secrets;

public sealed class SecretLogging
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public void LogDecision(ModeExecutionResult result)
    {
        var payload = new
        {
            correlationId = result.CorrelationId,
            mode = result.Mode.ToString().ToLowerInvariant(),
            guardrailsSatisfied = result.GuardrailsSatisfied,
            redactionApplied = result.RedactionApplied,
            allowedKeyEnforcement = result.AllowedKeyEnforcementApplied,
            totals = new
            {
                total = result.Items.Count,
                changes = result.Items.Count(item => item.Outcome == SecretHandlingOutcome.Allowed),
                skips = result.Items.Count(item => item.Outcome == SecretHandlingOutcome.Skipped),
                failures = result.Items.Count(item => item.Outcome == SecretHandlingOutcome.Failed)
            }
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        Console.WriteLine(json);
    }
}
