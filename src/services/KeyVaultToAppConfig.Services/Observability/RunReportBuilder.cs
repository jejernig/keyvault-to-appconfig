using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Errors;
using KeyVaultToAppConfig.Core.Observability;

namespace KeyVaultToAppConfig.Services.Observability;

public sealed class RunReportBuilder
{
    public RunReportModel Build(RunReport report, VerbosityLevel verbosity, string correlationId)
    {
        var items = new List<RunReportItem>();
        foreach (var change in DeterministicOrdering.OrderChanges(report.Changes))
        {
            items.Add(new RunReportItem
            {
                Key = Redaction.Redact(change.Key),
                Label = string.IsNullOrWhiteSpace(change.Label) ? null : Redaction.Redact(change.Label),
                Outcome = "changed",
                Reason = Redaction.Redact(change.Reason),
                Timestamp = report.Timestamp
            });
        }

        foreach (var failure in report.Failures.OrderBy(f => f.Key, StringComparer.OrdinalIgnoreCase))
        {
            items.Add(new RunReportItem
            {
                Key = Redaction.Redact(failure.Key),
                Label = null,
                Outcome = "failed",
                Reason = Redaction.Redact(failure.Message),
                Timestamp = report.Timestamp
            });
        }

        var failures = report.Failures
            .OrderBy(failure => failure.Key, StringComparer.OrdinalIgnoreCase)
            .Select(failure => new RunReportFailure
            {
                Key = Redaction.Redact(failure.Key),
                Label = null,
                ErrorType = failure.ErrorType,
                Message = Redaction.Redact(failure.Message)
            }).ToList();

        var secretOutcomes = report.SecretOutcomes
            .OrderBy(outcome => outcome.SecretKey, StringComparer.OrdinalIgnoreCase)
            .Select(outcome => new RunReportSecretOutcome
            {
                Key = Redaction.Redact(outcome.SecretKey),
                Status = outcome.Status.ToString().ToLowerInvariant(),
                ErrorId = outcome.ErrorId
            }).ToList();

        var errors = report.Errors
            .OrderBy(error => error.ErrorId, StringComparer.OrdinalIgnoreCase)
            .Select(error => new RunReportError
            {
                ErrorId = error.ErrorId,
                Classification = error.Classification.ToString().ToLowerInvariant(),
                Scope = error.Scope,
                Stage = error.Stage,
                Summary = Redaction.Redact(error.Summary),
                OccurredAt = error.OccurredAt
            }).ToList();

        return new RunReportModel
        {
            CorrelationId = correlationId,
            RunMode = report.ExecutionMode.ToString().ToLowerInvariant(),
            Verbosity = verbosity.ToString().ToLowerInvariant(),
            RunOutcome = report.Outcome.ToString().ToLowerInvariant(),
            ExitCode = report.ExitCode,
            Totals = new RunReportTotals
            {
                Total = report.Totals.Scanned,
                Changes = report.Totals.Changed,
                Skips = report.Totals.Skipped,
                Failures = report.Totals.Failed
            },
            ErrorTotals = new RunReportErrorTotals
            {
                SuccessfulSecrets = report.ErrorTotals.SuccessfulSecrets,
                RecoverableFailures = report.ErrorTotals.RecoverableFailures,
                UnprocessedSecrets = report.ErrorTotals.UnprocessedSecrets
            },
            Items = items,
            SecretOutcomes = secretOutcomes,
            Failures = failures,
            Errors = errors,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }
}
