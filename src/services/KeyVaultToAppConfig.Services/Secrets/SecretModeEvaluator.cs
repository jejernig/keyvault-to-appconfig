using System.Linq;
using KeyVaultToAppConfig.Core.Secrets;

namespace KeyVaultToAppConfig.Services.Secrets;

public sealed class SecretModeEvaluator : ISecretModeEvaluator
{
    private readonly SecretUriResolver _resolver;
    private readonly CopyValueGuardrailEvaluator _guardrailEvaluator;
    private readonly SecretRedaction _redaction;

    public SecretModeEvaluator()
        : this(new SecretUriResolver(), new CopyValueGuardrailEvaluator(), new SecretRedaction())
    {
    }

    public SecretModeEvaluator(
        SecretUriResolver resolver,
        CopyValueGuardrailEvaluator guardrailEvaluator,
        SecretRedaction redaction)
    {
        _resolver = resolver;
        _guardrailEvaluator = guardrailEvaluator;
        _redaction = redaction;
    }

    public Task<ModeExecutionResult> EvaluateAsync(SecretModeRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var correlationId = string.IsNullOrWhiteSpace(request.CorrelationId)
            ? Guid.NewGuid().ToString("n")
            : request.CorrelationId;

        var redactionApplied = request.RedactionPolicy.RedactValues || request.RedactionPolicy.RedactNames;
        var guardrailResult = request.Mode == SecretHandlingMode.CopyValue
            ? _guardrailEvaluator.Evaluate(request.Guardrail)
            : new CopyValueGuardrailResult { GuardrailsSatisfied = true };

        var items = new List<SecretItemOutcome>();
        foreach (var uri in request.SecretUris)
        {
            var resolution = _resolver.Resolve(uri);
            var key = resolution.SecretName ?? resolution.OriginalUri;
            var allowed = request.Mode != SecretHandlingMode.CopyValue ||
                          (guardrailResult.GuardrailsSatisfied && guardrailResult.AllowedKeys.Contains(key));

            var outcome = BuildOutcome(request.Mode, guardrailResult.GuardrailsSatisfied, allowed, resolution);

            items.Add(new SecretItemOutcome
            {
                Key = key,
                OriginalUri = resolution.OriginalUri,
                Mode = request.Mode,
                GuardrailsSatisfied = guardrailResult.GuardrailsSatisfied,
                AllowedKey = allowed,
                ResolvedVersion = resolution.Version,
                IsValid = resolution.IsValid,
                FailureReason = resolution.FailureReason,
                Outcome = outcome
            });
        }

        var result = new ModeExecutionResult
        {
            CorrelationId = correlationId,
            Mode = request.Mode,
            GuardrailsSatisfied = guardrailResult.GuardrailsSatisfied,
            RedactionApplied = redactionApplied,
            AllowedKeyEnforcementApplied = request.Mode == SecretHandlingMode.CopyValue,
            Items = ApplyRedaction(items, request.RedactionPolicy)
        };

        if (request.Mode == SecretHandlingMode.CopyValue && !guardrailResult.GuardrailsSatisfied)
        {
            result.Messages.Add(
                ApplyRedaction("Copy-value guardrails not satisfied.", request.RedactionPolicy));
        }

        return Task.FromResult(result);
    }

    private List<SecretItemOutcome> ApplyRedaction(IEnumerable<SecretItemOutcome> items, RedactionPolicy policy)
    {
        return items.Select(item => new SecretItemOutcome
        {
            Key = _redaction.RedactName(item.Key, policy),
            OriginalUri = policy.RedactNames ? "[REDACTED]" : item.OriginalUri,
            Mode = item.Mode,
            GuardrailsSatisfied = item.GuardrailsSatisfied,
            AllowedKey = item.AllowedKey,
            ResolvedVersion = item.ResolvedVersion,
            IsValid = item.IsValid,
            FailureReason = policy.RedactValues
                ? Core.Redaction.Redact(item.FailureReason)
                : item.FailureReason,
            Outcome = item.Outcome
        }).ToList();
    }

    private string ApplyRedaction(string message, RedactionPolicy policy)
    {
        return policy.RedactValues
            ? Core.Redaction.Redact(message)
            : message;
    }

    private static SecretHandlingOutcome BuildOutcome(
        SecretHandlingMode mode,
        bool guardrailsSatisfied,
        bool allowed,
        SecretUriResolution resolution)
    {
        if (!resolution.IsValid)
        {
            return SecretHandlingOutcome.Failed;
        }

        if (mode == SecretHandlingMode.CopyValue && (!guardrailsSatisfied || !allowed))
        {
            return SecretHandlingOutcome.Skipped;
        }

        return SecretHandlingOutcome.Allowed;
    }
}
