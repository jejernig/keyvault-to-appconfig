using System.Linq;
using System.Text.Json;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Enumeration;

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
            EnumeratedSecrets = report.EnumeratedSecrets.Select(secret => new SecretDescriptor
            {
                Name = Redaction.Redact(secret.Name),
                Version = secret.Version,
                IsEnabled = secret.IsEnabled,
                Tags = RedactTags(secret.Tags),
                LastUpdated = secret.LastUpdated
            }).ToList(),
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

    private static IDictionary<string, string> RedactTags(IDictionary<string, string> tags)
    {
        var redacted = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in tags)
        {
            redacted[entry.Key] = Redaction.Redact(entry.Value);
        }

        return redacted;
    }
}
