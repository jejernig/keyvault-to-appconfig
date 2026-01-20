using KeyVaultToAppConfig.Cli;
using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.UnitTests;

public sealed class ReportRedactionTests
{
    [Fact]
    public async Task WriteJsonAsync_RedactsValueMarkers()
    {
        var report = new RunReport
        {
            RunId = "test",
            ExecutionMode = ExecutionMode.DryRun,
            Totals = new Totals { Scanned = 1 },
            Changes =
            {
                new ChangeSummary
                {
                    Key = "app/setting",
                    Action = "update",
                    Reason = "value=supersecret"
                }
            },
            Failures =
            {
                new FailureSummary
                {
                    Key = "app/failed",
                    ErrorType = "error",
                    Message = "value: supersecret"
                }
            }
        };

        var outputPath = Path.Combine(Path.GetTempPath(), $"report-{Guid.NewGuid():N}.json");
        var writer = new ReportWriter();

        try
        {
            await writer.WriteJsonAsync(report, outputPath, CancellationToken.None);
            var content = await File.ReadAllTextAsync(outputPath);

            Assert.DoesNotContain("supersecret", content, StringComparison.Ordinal);
            Assert.Contains("[REDACTED]", content, StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }
}
