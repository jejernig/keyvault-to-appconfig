using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Observability;

namespace KeyVaultToAppConfig.Services.Observability;

public interface IObservabilityReporter
{
    string CorrelationId { get; }
    void Log(LogEntry entry);
    ConsoleSummary BuildSummary(RunReport report);
    RunReportModel BuildReport(RunReport report, VerbosityLevel verbosity);
}
