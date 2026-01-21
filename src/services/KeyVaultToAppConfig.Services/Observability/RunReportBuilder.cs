using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Observability;

namespace KeyVaultToAppConfig.Services.Observability;

public sealed class RunReportBuilder
{
    public RunReportModel Build(RunReport report, VerbosityLevel verbosity, string correlationId)
    {
        var items = new List<RunReportItem>();
        foreach (var change in report.Changes)
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

        foreach (var failure in report.Failures)
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

        var failures = report.Failures.Select(failure => new RunReportFailure
        {
            Key = Redaction.Redact(failure.Key),
            Label = null,
            ErrorType = failure.ErrorType,
            Message = Redaction.Redact(failure.Message)
        }).ToList();

        return new RunReportModel
        {
            CorrelationId = correlationId,
            RunMode = report.ExecutionMode.ToString().ToLowerInvariant(),
            Verbosity = verbosity.ToString().ToLowerInvariant(),
            Totals = new RunReportTotals
            {
                Total = report.Totals.Scanned,
                Changes = report.Totals.Changed,
                Skips = report.Totals.Skipped,
                Failures = report.Totals.Failed
            },
            Items = items,
            Failures = failures,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }
}
