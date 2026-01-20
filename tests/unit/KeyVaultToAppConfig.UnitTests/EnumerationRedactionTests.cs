using System.Text.Json;
using KeyVaultToAppConfig.Cli;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Enumeration;

namespace KeyVaultToAppConfig.UnitTests;

public sealed class EnumerationRedactionTests
{
    [Fact]
    public async Task ReportWriterRedactsEnumeratedSecretNames()
    {
        var report = new RunReport
        {
            RunId = "test",
            ExecutionMode = ExecutionMode.DryRun,
            EnumeratedSecrets = new List<SecretDescriptor>
            {
                new()
                {
                    Name = "password=supersecret",
                    Version = "v1",
                    IsEnabled = true,
                    Tags = new Dictionary<string, string>(),
                    LastUpdated = DateTimeOffset.UtcNow
                }
            }
        };

        var outputPath = Path.Combine(Path.GetTempPath(), $"enum-report-{Guid.NewGuid():N}.json");
        try
        {
            var writer = new ReportWriter();
            await writer.WriteJsonAsync(report, outputPath, CancellationToken.None);

            var json = await File.ReadAllTextAsync(outputPath);
            using var doc = JsonDocument.Parse(json);
            var name = doc.RootElement
                .GetProperty("EnumeratedSecrets")[0]
                .GetProperty("Name")
                .GetString();

            Assert.Equal("password=[REDACTED]", name);
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
