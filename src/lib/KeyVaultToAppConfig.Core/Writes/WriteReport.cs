namespace KeyVaultToAppConfig.Core.Writes;

public sealed class WriteReport
{
    public string CorrelationId { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CompletedAt { get; set; }
    public WriteTotals Totals { get; set; } = new();
    public List<WriteResult> Results { get; set; } = new();
}
