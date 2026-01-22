using KeyVaultToAppConfig.Core.Enumeration;
using KeyVaultToAppConfig.Core.Errors;

namespace KeyVaultToAppConfig.Core;

public sealed class RunReport
{
    public string RunId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public ExecutionMode ExecutionMode { get; set; }
    public RunOutcome Outcome { get; set; } = RunOutcome.Success;
    public int ExitCode { get; set; }
    public Totals Totals { get; set; } = new();
    public ErrorTotals ErrorTotals { get; set; } = new();
    public List<ChangeSummary> Changes { get; set; } = new();
    public List<FailureSummary> Failures { get; set; } = new();
    public List<ErrorRecord> Errors { get; set; } = new();
    public List<SecretOperationOutcome> SecretOutcomes { get; set; } = new();
    public List<SecretDescriptor> EnumeratedSecrets { get; set; } = new();
}

public sealed class Totals
{
    public int Scanned { get; set; }
    public int Changed { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
}

public sealed class ErrorTotals
{
    public int SuccessfulSecrets { get; set; }
    public int RecoverableFailures { get; set; }
    public int UnprocessedSecrets { get; set; }
}
