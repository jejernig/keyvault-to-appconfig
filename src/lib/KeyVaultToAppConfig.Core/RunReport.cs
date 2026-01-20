using KeyVaultToAppConfig.Core.Enumeration;

namespace KeyVaultToAppConfig.Core;

public sealed class RunReport
{
    public string RunId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public ExecutionMode ExecutionMode { get; set; }
    public Totals Totals { get; set; } = new();
    public List<ChangeSummary> Changes { get; set; } = new();
    public List<FailureSummary> Failures { get; set; } = new();
    public List<SecretDescriptor> EnumeratedSecrets { get; set; } = new();
}

public sealed class Totals
{
    public int Scanned { get; set; }
    public int Changed { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
}
