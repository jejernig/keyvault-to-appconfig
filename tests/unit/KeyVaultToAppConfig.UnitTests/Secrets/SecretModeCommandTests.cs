using System;
using System.CommandLine;
using System.Text.Json;
using KeyVaultToAppConfig.Cli;
using KeyVaultToAppConfig.Cli.Commands;
using KeyVaultToAppConfig.Core.Secrets;

namespace KeyVaultToAppConfig.UnitTests.Secrets;

public sealed class SecretModeCommandTests
{
    [Fact]
    public async Task WriteSecretHandlingReportJsonAsync_IncludesCountsAndBuckets()
    {
        var result = new ModeExecutionResult
        {
            CorrelationId = "corr-1",
            Mode = SecretHandlingMode.Reference,
            Items =
            {
                new SecretItemOutcome { Key = "a", Outcome = SecretHandlingOutcome.Allowed },
                new SecretItemOutcome { Key = "b", Outcome = SecretHandlingOutcome.Skipped },
                new SecretItemOutcome { Key = "c", Outcome = SecretHandlingOutcome.Failed }
            }
        };

        var outputPath = Path.Combine(Path.GetTempPath(), $"secret-mode-{Guid.NewGuid():N}.json");
        var writer = new ReportWriter();

        await writer.WriteSecretHandlingReportJsonAsync(result, outputPath, CancellationToken.None);

        var json = await File.ReadAllTextAsync(outputPath);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("correlationId", out _));
        Assert.True(root.TryGetProperty("totals", out var totals));
        Assert.Equal(3, totals.GetProperty("total").GetInt32());
        Assert.Equal(1, totals.GetProperty("changes").GetInt32());
        Assert.Equal(1, totals.GetProperty("skips").GetInt32());
        Assert.Equal(1, totals.GetProperty("failures").GetInt32());
    }

    [Fact]
    public async Task SecretModeCommand_ReturnsFailureWhenAnyItemFails()
    {
        var command = SecretModeCommand.Build();
        var parseResult = command.Parse(new[] { "--mode", "reference", "--secret-uri", "not-a-uri" });
        var exitCode = await parseResult.InvokeAsync();

        Assert.Equal(1, exitCode);
    }
}
