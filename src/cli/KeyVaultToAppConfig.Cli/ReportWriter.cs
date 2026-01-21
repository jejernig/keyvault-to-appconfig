using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Enumeration;
using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Core.Writes;

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

    public async Task WritePlanJsonAsync(
        PlanOutput plan,
        string correlationId,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var sanitizedReport = RedactPlanReport(plan, correlationId);
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(stream, sanitizedReport, JsonOptions, cancellationToken);
    }

    public async Task WriteWriteReportJsonAsync(
        WriteReport report,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var sanitizedReport = RedactWriteReport(report);
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

    private static PlanReport RedactPlanReport(PlanOutput plan, string correlationId)
    {
        return new PlanReport
        {
            CorrelationId = correlationId,
            GeneratedAt = plan.GeneratedAt,
            Totals = plan.Totals,
            Changes = plan.DiffItems.Select(item => new ChangeSummary
            {
                Key = item.Key,
                Label = item.Label,
                Action = item.Classification.ToString().ToLowerInvariant(),
                Reason = Redaction.Redact(item.Reason)
            }).ToList(),
            Skips = plan.Conflicts.Select(conflict => new PlanSkip
            {
                Key = conflict.Key,
                Label = conflict.Label,
                Reason = "Conflict"
            }).ToList(),
            Failures = new List<FailureSummary>()
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

    private static WriteReportSummary RedactWriteReport(WriteReport report)
    {
        var changes = report.Results
            .Where(result => result.Status == WriteStatus.Succeeded)
            .Select(result => new WriteResultSummary
            {
                Key = Redaction.Redact(result.Key),
                Label = Redaction.Redact(result.Label),
                Status = result.Status.ToString().ToLowerInvariant(),
                Attempts = result.Attempts,
                RetryCount = result.RetryCount
            })
            .ToList();

        var skips = report.Results
            .Where(result => result.Status == WriteStatus.Skipped)
            .Select(result => new WriteResultSummary
            {
                Key = Redaction.Redact(result.Key),
                Label = Redaction.Redact(result.Label),
                Status = result.Status.ToString().ToLowerInvariant(),
                Attempts = result.Attempts,
                RetryCount = result.RetryCount
            })
            .ToList();

        var failures = report.Results
            .Where(result => result.Status is WriteStatus.Failed or WriteStatus.RolledBack)
            .Select(result => new WriteResultSummary
            {
                Key = Redaction.Redact(result.Key),
                Label = Redaction.Redact(result.Label),
                Status = result.Status.ToString().ToLowerInvariant(),
                Attempts = result.Attempts,
                RetryCount = result.RetryCount,
                FailureReason = Redaction.Redact(result.FailureReason)
            })
            .ToList();

        return new WriteReportSummary
        {
            CorrelationId = report.CorrelationId,
            StartedAt = report.StartedAt,
            CompletedAt = report.CompletedAt,
            Totals = report.Totals,
            Changes = changes,
            Skips = skips,
            Failures = failures
        };
    }

    private sealed class PlanReport
    {
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonPropertyName("generatedAt")]
        public DateTimeOffset GeneratedAt { get; set; }

        [JsonPropertyName("totals")]
        public PlanTotals Totals { get; set; } = new();

        [JsonPropertyName("changes")]
        public List<ChangeSummary> Changes { get; set; } = new();

        [JsonPropertyName("skips")]
        public List<PlanSkip> Skips { get; set; } = new();

        [JsonPropertyName("failures")]
        public List<FailureSummary> Failures { get; set; } = new();
    }

    private sealed class PlanSkip
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }

    private sealed class WriteReportSummary
    {
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonPropertyName("startedAt")]
        public DateTimeOffset StartedAt { get; set; }

        [JsonPropertyName("completedAt")]
        public DateTimeOffset CompletedAt { get; set; }

        [JsonPropertyName("totals")]
        public WriteTotals Totals { get; set; } = new();

        [JsonPropertyName("changes")]
        public List<WriteResultSummary> Changes { get; set; } = new();

        [JsonPropertyName("skips")]
        public List<WriteResultSummary> Skips { get; set; } = new();

        [JsonPropertyName("failures")]
        public List<WriteResultSummary> Failures { get; set; } = new();
    }

    private sealed class WriteResultSummary
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("attempts")]
        public int Attempts { get; set; }

        [JsonPropertyName("retryCount")]
        public int RetryCount { get; set; }

        [JsonPropertyName("failureReason")]
        public string? FailureReason { get; set; }
    }
}
