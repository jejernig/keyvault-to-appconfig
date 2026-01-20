using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.Cli;

public sealed class OutputWriter
{
    public void WriteValidationErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            Console.Error.WriteLine(error);
        }
    }

    public void WriteUsage(string usage)
    {
        Console.WriteLine(usage);
    }

    public void WriteRunSummary(RunReport report)
    {
        Console.WriteLine($"Run {report.RunId} ({report.ExecutionMode})");
        Console.WriteLine($"Scanned: {report.Totals.Scanned}");
        Console.WriteLine($"Changed: {report.Totals.Changed}");
        Console.WriteLine($"Skipped: {report.Totals.Skipped}");
        Console.WriteLine($"Failed: {report.Totals.Failed}");
    }

    public void WriteDiff(IEnumerable<ChangeSummary> changes)
    {
        foreach (var change in changes)
        {
            var label = string.IsNullOrWhiteSpace(change.Label) ? string.Empty : $" [{change.Label}]";
            Console.WriteLine($"{change.Action}: {change.Key}{label} - {Redaction.Redact(change.Reason)}");
        }
    }
}
