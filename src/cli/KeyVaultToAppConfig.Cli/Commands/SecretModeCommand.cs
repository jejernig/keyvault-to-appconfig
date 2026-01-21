using System.CommandLine;
using System.Linq;
using KeyVaultToAppConfig.Core.Secrets;
using KeyVaultToAppConfig.Services.Secrets;
using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.Cli.Commands;

public static class SecretModeCommand
{
    private const int SuccessExitCode = 0;
    private const int FailureExitCode = 1;

    public static Command Build()
    {
        var modeOption = new Option<string>("--mode") { Required = true };
        var secretUriOption = new Option<string[]>("--secret-uri") { AllowMultipleArgumentsPerToken = true };
        var secretUriPathOption = new Option<string?>("--secret-uri-path");
        var confirmCopyValueOption = new Option<bool>("--confirm-copy-value");
        var confirmCopyValueTextOption = new Option<string?>("--confirm-copy-value-text");
        var allowedKeyOption = new Option<string[]>("--allowed-key") { AllowMultipleArgumentsPerToken = true };
        var redactNamesOption = new Option<bool>("--redact-names");
        var reportJsonOption = new Option<string?>("--report-json");

        var command = new Command("secret-mode", "Evaluate secret handling mode and guardrails")
        {
            modeOption,
            secretUriOption,
            secretUriPathOption,
            confirmCopyValueOption,
            confirmCopyValueTextOption,
            allowedKeyOption,
            redactNamesOption,
            reportJsonOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var modeText = parseResult.GetRequiredValue(modeOption);
            if (!TryParseMode(modeText, out var mode))
            {
                Console.Error.WriteLine("mode: Must be 'reference' or 'copyValue'.");
                return FailureExitCode;
            }

            var secretUris = new List<string>();
            secretUris.AddRange(parseResult.GetValue(secretUriOption) ?? Array.Empty<string>());

            var secretUriPath = parseResult.GetValue(secretUriPathOption);
            if (!string.IsNullOrWhiteSpace(secretUriPath))
            {
                if (!File.Exists(secretUriPath))
                {
                    Console.Error.WriteLine("secret-uri-path: File not found.");
                    return FailureExitCode;
                }

                var fileUris = File.ReadAllLines(secretUriPath)
                    .Where(line => !string.IsNullOrWhiteSpace(line));
                secretUris.AddRange(fileUris);
            }

            if (secretUris.Count == 0)
            {
                Console.Error.WriteLine("secret-uri: At least one secret URI is required.");
                return FailureExitCode;
            }

            var guardrail = new CopyValueGuardrail
            {
                FlagProvided = parseResult.GetValue(confirmCopyValueOption),
                SecondaryConfirmation = parseResult.GetValue(confirmCopyValueTextOption),
                AllowedKeys = (parseResult.GetValue(allowedKeyOption) ?? Array.Empty<string>()).ToList()
            };

            var redactionPolicy = new RedactionPolicy
            {
                RedactValues = true,
                RedactNames = parseResult.GetValue(redactNamesOption)
            };

            var request = new SecretModeRequest
            {
                Mode = mode,
                SecretUris = secretUris,
                Guardrail = guardrail,
                RedactionPolicy = redactionPolicy,
                CorrelationId = Guid.NewGuid().ToString("n")
            };

            var evaluator = new SecretModeEvaluator();
            var result = await evaluator.EvaluateAsync(request, cancellationToken);

            var redactor = new SecretRedaction();
            var redacted = ApplyRedaction(result, redactionPolicy, redactor);

            var logger = new SecretLogging();
            logger.LogDecision(redacted);

            WriteSummary(redacted);

            var reportPath = parseResult.GetValue(reportJsonOption);
            if (!string.IsNullOrWhiteSpace(reportPath))
            {
                var writer = new ReportWriter();
                await writer.WriteSecretHandlingReportJsonAsync(redacted, reportPath, cancellationToken);
            }

            return redacted.Items.Any(item => item.Outcome == SecretHandlingOutcome.Failed)
                ? FailureExitCode
                : SuccessExitCode;
        });

        return command;
    }

    private static ModeExecutionResult ApplyRedaction(
        ModeExecutionResult result,
        RedactionPolicy policy,
        SecretRedaction redactor)
    {
        var redactedItems = result.Items.Select(item => new SecretItemOutcome
        {
            Key = redactor.RedactName(item.Key, policy),
            OriginalUri = policy.RedactNames ? "[REDACTED]" : item.OriginalUri,
            Mode = item.Mode,
            GuardrailsSatisfied = item.GuardrailsSatisfied,
            AllowedKey = item.AllowedKey,
            ResolvedVersion = item.ResolvedVersion,
            IsValid = item.IsValid,
            FailureReason = policy.RedactValues ? Redaction.Redact(item.FailureReason) : item.FailureReason,
            Outcome = item.Outcome
        }).ToList();

        return new ModeExecutionResult
        {
            CorrelationId = result.CorrelationId,
            Mode = result.Mode,
            GuardrailsSatisfied = result.GuardrailsSatisfied,
            RedactionApplied = result.RedactionApplied,
            AllowedKeyEnforcementApplied = result.AllowedKeyEnforcementApplied,
            Items = redactedItems,
            Messages = result.Messages.Select(message => policy.RedactValues ? Redaction.Redact(message) : message).ToList()
        };
    }

    private static void WriteSummary(ModeExecutionResult result)
    {
        var allowed = result.Items.Count(item => item.Outcome == SecretHandlingOutcome.Allowed);
        var skipped = result.Items.Count(item => item.Outcome == SecretHandlingOutcome.Skipped);
        var failed = result.Items.Count(item => item.Outcome == SecretHandlingOutcome.Failed);

        Console.WriteLine($"correlation-id: {result.CorrelationId}");
        Console.WriteLine($"mode: {result.Mode}");
        Console.WriteLine($"guardrails-satisfied: {result.GuardrailsSatisfied}");
        Console.WriteLine($"allowed: {allowed}");
        Console.WriteLine($"skipped: {skipped}");
        Console.WriteLine($"failed: {failed}");
    }

    private static bool TryParseMode(string value, out SecretHandlingMode mode)
    {
        if (string.Equals(value, "reference", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "kvref", StringComparison.OrdinalIgnoreCase))
        {
            mode = SecretHandlingMode.Reference;
            return true;
        }

        if (string.Equals(value, "copyvalue", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "copy-value", StringComparison.OrdinalIgnoreCase))
        {
            mode = SecretHandlingMode.CopyValue;
            return true;
        }

        mode = SecretHandlingMode.Reference;
        return false;
    }
}
