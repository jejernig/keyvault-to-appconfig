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

    [JsonPropertyName("totals")]
    public RunReportTotals Totals { get; set; } = new();

    [JsonPropertyName("items")]
    public List<RunReportItem> Items { get; set; } = new();

    [JsonPropertyName("failures")]
    public List<RunReportFailure> Failures { get; set; } = new();

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
