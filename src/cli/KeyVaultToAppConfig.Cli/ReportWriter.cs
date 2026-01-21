using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Enumeration;
using KeyVaultToAppConfig.Core.Planning;

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
}
