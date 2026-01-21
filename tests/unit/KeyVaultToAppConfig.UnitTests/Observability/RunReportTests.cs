using KeyVaultToAppConfig.Cli;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Observability;
using KeyVaultToAppConfig.Services.Observability;

namespace KeyVaultToAppConfig.UnitTests.Observability;

public sealed class RunReportTests
{
    [Fact]
    public void Build_ReturnsTotalsAndItems()
    {
        var report = new RunReport
        {
            RunId = "corr-1",
            ExecutionMode = ExecutionMode.DryRun,
            Timestamp = DateTimeOffset.UtcNow,
            Totals = new Totals { Scanned = 3, Changed = 1, Skipped = 1, Failed = 1 },
            Changes = { new ChangeSummary { Key = "k1", Action = "update", Reason = "changed" } },
            Failures = { new FailureSummary { Key = "k2", ErrorType = "error", Message = "failure" } }
        };

        var builder = new RunReportBuilder();
        var model = builder.Build(report, VerbosityLevel.Normal, report.RunId);

        Assert.Equal("corr-1", model.CorrelationId);
        Assert.Equal(3, model.Totals.Total);
        Assert.Equal(2, model.Items.Count);
        Assert.Single(model.Failures);
    }

    [Fact]
    public async Task WriteObservabilityReportJsonAsync_IsDeterministic()
    {
        var report = new RunReportModel
        {
            CorrelationId = "corr-1",
            RunMode = "dryrun",
            Verbosity = "normal",
            Totals = new RunReportTotals { Total = 1, Changes = 0, Skips = 0, Failures = 0 },
            Items = { new RunReportItem { Key = "k1", Outcome = "unchanged", Timestamp = DateTimeOffset.UtcNow } },
            Failures = new List<RunReportFailure>(),
            GeneratedAt = new DateTimeOffset(2026, 1, 21, 0, 0, 0, TimeSpan.Zero)
        };

        var writer = new ReportWriter();
        var path1 = Path.Combine(Path.GetTempPath(), $"report-a-{Guid.NewGuid():N}.json");
        var path2 = Path.Combine(Path.GetTempPath(), $"report-b-{Guid.NewGuid():N}.json");

        await writer.WriteObservabilityReportJsonAsync(report, path1, CancellationToken.None);
        await writer.WriteObservabilityReportJsonAsync(report, path2, CancellationToken.None);

        var json1 = await File.ReadAllTextAsync(path1);
        var json2 = await File.ReadAllTextAsync(path2);

        Assert.Equal(json1, json2);
    }
}
