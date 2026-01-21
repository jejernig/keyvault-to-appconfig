using System.Text.Json;
using KeyVaultToAppConfig.Cli;
using KeyVaultToAppConfig.Core.Writes;

namespace KeyVaultToAppConfig.UnitTests.Writes;

public sealed class WriteReportTests
{
    [Fact]
    public async Task WriteWriteReportJsonAsync_IncludesCountsAndBuckets()
    {
        var report = new WriteReport
        {
            CorrelationId = "corr-1",
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            CompletedAt = DateTimeOffset.UtcNow,
            Totals = new WriteTotals
            {
                CreateCount = 1,
                UpdateCount = 1,
                SkipCount = 1,
                FailedCount = 1
            },
            Results =
            {
                new WriteResult { Key = "a", Label = "prod", Status = WriteStatus.Succeeded, Attempts = 1 },
                new WriteResult { Key = "b", Label = "prod", Status = WriteStatus.Skipped, Attempts = 0 },
                new WriteResult { Key = "c", Label = "prod", Status = WriteStatus.Failed, Attempts = 2 }
            }
        };

        var outputPath = Path.Combine(Path.GetTempPath(), $"write-report-{Guid.NewGuid():N}.json");
        var writer = new ReportWriter();

        await writer.WriteWriteReportJsonAsync(report, outputPath, CancellationToken.None);

        var json = await File.ReadAllTextAsync(outputPath);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("correlationId", out _));
        Assert.True(root.TryGetProperty("totals", out _));
        Assert.True(root.TryGetProperty("changes", out var changes));
        Assert.True(root.TryGetProperty("skips", out var skips));
        Assert.True(root.TryGetProperty("failures", out var failures));
        Assert.Equal(1, changes.GetArrayLength());
        Assert.Equal(1, skips.GetArrayLength());
        Assert.Equal(1, failures.GetArrayLength());
    }
}
