using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Services.Planning;
using KeyVaultToAppConfig.Cli;
using System.Text.Json;

namespace KeyVaultToAppConfig.UnitTests.Planning;

public sealed class PlanningEngineTests
{
    [Fact]
    public async Task BuildPlanAsync_IsDeterministic()
    {
        var desiredState = new DesiredState
        {
            Entries =
            {
                new DesiredEntry { Key = "b-key", Label = "prod", Value = "2" },
                new DesiredEntry { Key = "a-key", Label = "prod", Value = "1" },
                new DesiredEntry { Key = "a-key", Label = "dev", Value = "3" }
            }
        };

        var existingState = new ExistingStateSnapshot
        {
            Entries =
            {
                new ExistingEntry { Key = "a-key", Label = "prod", Value = "1" }
            }
        };

        var engine = new PlanningEngine();
        var first = await engine.BuildPlanAsync(desiredState, existingState, CancellationToken.None);
        var second = await engine.BuildPlanAsync(desiredState, existingState, CancellationToken.None);

        Assert.Equal(
            first.DiffItems.Select(item => $"{item.Classification}:{item.Key}:{item.Label}"),
            second.DiffItems.Select(item => $"{item.Classification}:{item.Key}:{item.Label}"));
    }

    [Fact]
    public async Task BuildPlanAsync_ClassifiesCreateUpdateAndUnchanged()
    {
        var desiredState = new DesiredState
        {
            Entries =
            {
                new DesiredEntry { Key = "alpha", Label = "prod", Value = "same" },
                new DesiredEntry { Key = "beta", Label = "prod", Value = "new" },
                new DesiredEntry { Key = "gamma", Label = "prod", Value = "updated", ContentType = "text/plain" }
            }
        };

        var existingState = new ExistingStateSnapshot
        {
            Entries =
            {
                new ExistingEntry { Key = "alpha", Label = "prod", Value = "same" },
                new ExistingEntry { Key = "gamma", Label = "prod", Value = "old", ContentType = "text/plain" }
            }
        };

        var engine = new PlanningEngine();
        var plan = await engine.BuildPlanAsync(desiredState, existingState, CancellationToken.None);

        var classifications = plan.DiffItems.ToDictionary(
            item => $"{item.Key}:{item.Label}",
            item => item.Classification);

        Assert.Equal(DiffClassification.Unchanged, classifications["alpha:prod"]);
        Assert.Equal(DiffClassification.Create, classifications["beta:prod"]);
        Assert.Equal(DiffClassification.Update, classifications["gamma:prod"]);
    }

    [Fact]
    public async Task WritePlanJsonAsync_IncludesCountsAndChanges()
    {
        var plan = new PlanOutput
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            DiffItems = new List<DiffItem>
            {
                new() { Key = "alpha", Label = "prod", Classification = DiffClassification.Create, Reason = "Missing" }
            },
            Conflicts = new List<ConflictRecord>(),
            Totals = new PlanTotals
            {
                CreateCount = 1,
                UpdateCount = 0,
                UnchangedCount = 0,
                ConflictCount = 0
            }
        };

        var reportWriter = new ReportWriter();
        var outputPath = Path.Combine(Path.GetTempPath(), $"plan-report-{Guid.NewGuid():N}.json");

        await reportWriter.WritePlanJsonAsync(plan, "corr-123", outputPath, CancellationToken.None);

        var json = await File.ReadAllTextAsync(outputPath);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("correlationId", out _));
        Assert.True(root.TryGetProperty("totals", out _));
        Assert.True(root.TryGetProperty("changes", out var changes));
        Assert.Equal(1, changes.GetArrayLength());
    }
}
