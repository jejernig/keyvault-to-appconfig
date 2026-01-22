using System.Text.Json.Serialization;

namespace KeyVaultToAppConfig.Core.Observability;

public sealed class RunReportModel
{
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;

    [JsonPropertyName("runMode")]
    public string RunMode { get; set; } = string.Empty;

    [JsonPropertyName("verbosity")]
    public string Verbosity { get; set; } = string.Empty;

    [JsonPropertyName("runOutcome")]
    public string RunOutcome { get; set; } = string.Empty;

    [JsonPropertyName("exitCode")]
    public int ExitCode { get; set; }

    [JsonPropertyName("totals")]
    public RunReportTotals Totals { get; set; } = new();

    [JsonPropertyName("errorTotals")]
    public RunReportErrorTotals ErrorTotals { get; set; } = new();

    [JsonPropertyName("items")]
    public List<RunReportItem> Items { get; set; } = new();

    [JsonPropertyName("secretOutcomes")]
    public List<RunReportSecretOutcome> SecretOutcomes { get; set; } = new();

    [JsonPropertyName("failures")]
    public List<RunReportFailure> Failures { get; set; } = new();

    [JsonPropertyName("errors")]
    public List<RunReportError> Errors { get; set; } = new();

    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class RunReportTotals
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("changes")]
    public int Changes { get; set; }

    [JsonPropertyName("skips")]
    public int Skips { get; set; }

    [JsonPropertyName("failures")]
    public int Failures { get; set; }
}

public sealed class RunReportItem
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class RunReportSecretOutcome
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("errorId")]
    public string? ErrorId { get; set; }
}

public sealed class RunReportFailure
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("errorType")]
    public string ErrorType { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public sealed class RunReportErrorTotals
{
    [JsonPropertyName("successfulSecrets")]
    public int SuccessfulSecrets { get; set; }

    [JsonPropertyName("recoverableFailures")]
    public int RecoverableFailures { get; set; }

    [JsonPropertyName("unprocessedSecrets")]
    public int UnprocessedSecrets { get; set; }
}

public sealed class RunReportError
{
    [JsonPropertyName("errorId")]
    public string ErrorId { get; set; } = string.Empty;

    [JsonPropertyName("classification")]
    public string Classification { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("stage")]
    public string Stage { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("occurredAt")]
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
