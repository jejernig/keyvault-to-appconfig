using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Services.Planning;

namespace KeyVaultToAppConfig.UnitTests.Planning;

public sealed class MappingCoverageTests
{
    [Fact]
    public void Build_IncludesAllDesiredEntries()
    {
        var entries = new List<DesiredEntry>
        {
            new() { Key = "alpha", Label = "prod", Value = "1" },
            new() { Key = "beta", Label = "prod", Value = "2" }
        };

        var builder = new DesiredStateBuilder();
        var state = builder.Build(entries, "unit-test");

        Assert.Equal(2, state.Entries.Count);
        Assert.Contains(state.Entries, entry => entry.Key == "alpha" && entry.Label == "prod");
        Assert.Contains(state.Entries, entry => entry.Key == "beta" && entry.Label == "prod");
    }

    [Fact]
    public void BuildFromMapping_ExpandsEnvironmentLabels()
    {
        var run = new MappingRun
        {
            NormalizedKeys = new Dictionary<string, string>
            {
                ["app:alpha"] = "source-alpha",
                ["app:beta"] = "source-beta"
            }
        };

        var builder = new DesiredStateBuilder();
        var desiredState = builder.BuildFromMapping(
            run,
            new[] { "prod", "dev" },
            source => $"value-for-{source}");

        Assert.Equal(4, desiredState.Entries.Count);
        Assert.Contains(desiredState.Entries, entry => entry.Key == "app:alpha" && entry.Label == "prod");
        Assert.Contains(desiredState.Entries, entry => entry.Key == "app:alpha" && entry.Label == "dev");
        Assert.Contains(desiredState.Entries, entry => entry.Key == "app:beta" && entry.Label == "prod");
        Assert.Contains(desiredState.Entries, entry => entry.Key == "app:beta" && entry.Label == "dev");
    }
}
