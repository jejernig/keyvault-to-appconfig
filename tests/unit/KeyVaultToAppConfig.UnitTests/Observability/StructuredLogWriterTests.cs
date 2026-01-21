using System.Text.Json;
using KeyVaultToAppConfig.Core.Observability;
using KeyVaultToAppConfig.Services.Observability;

namespace KeyVaultToAppConfig.UnitTests.Observability;

public sealed class StructuredLogWriterTests
{
    [Fact]
    public void Write_EmitsJsonWithCorrelationId()
    {
        var entry = new LogEntry
        {
            CorrelationId = "corr-1",
            Event = "run-start",
            Message = "Run started.",
            Level = "info"
        };

        var writer = new StructuredLogWriter();
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);
        try
        {
            writer.Write(entry);

            var json = output.ToString().Trim();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal("corr-1", root.GetProperty("correlationId").GetString());
            Assert.Equal("run-start", root.GetProperty("event").GetString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}
