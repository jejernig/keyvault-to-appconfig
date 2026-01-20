using System.Linq;
using System.Text.Json;
using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.Cli;

public sealed class ReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task WriteJsonAsync(RunReport report, string outputPath, CancellationToken cancellationToken)
    {
        var sanitizedReport = RedactReport(report);
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(stream, sanitizedReport, JsonOptions, cancellationToken);
    }

    private static RunReport RedactReport(RunReport report)
    {
        return new RunReport
        {
            RunId = report.RunId,
            Timestamp = report.Timestamp,
            ExecutionMode = report.ExecutionMode,
            Totals = report.Totals,
            Changes = report.Changes.Select(change => new ChangeSummary
            {
                Key = change.Key,
                Label = change.Label,
                Action = change.Action,
                Reason = Redaction.Redact(change.Reason)
            }).ToList(),
            Failures = report.Failures.Select(failure => new FailureSummary
            {
                Key = failure.Key,
                ErrorType = failure.ErrorType,
                Message = Redaction.Redact(failure.Message)
            }).ToList()
        };
    }
}
