using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Observability;

namespace KeyVaultToAppConfig.Services.Observability;

public sealed class ObservabilityReporter : IObservabilityReporter
{
    private readonly StructuredLogWriter _logWriter;
    private readonly RunReportBuilder _reportBuilder;

    public ObservabilityReporter(string correlationId)
    {
        CorrelationId = correlationId;
        _logWriter = new StructuredLogWriter();
        _reportBuilder = new RunReportBuilder();
    }

    public string CorrelationId { get; }

    public void Log(LogEntry entry)
    {
        _logWriter.Write(entry);
    }

    public ConsoleSummary BuildSummary(RunReport report)
    {
        return new ConsoleSummary
        {
            CorrelationId = CorrelationId,
            Totals = report.Totals,
            EmittedAt = DateTimeOffset.UtcNow
        };
    }

    public RunReportModel BuildReport(RunReport report, VerbosityLevel verbosity)
    {
        return _reportBuilder.Build(report, verbosity, CorrelationId);
    }
}
