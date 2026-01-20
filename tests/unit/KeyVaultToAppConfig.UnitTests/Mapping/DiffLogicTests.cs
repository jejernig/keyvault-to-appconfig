using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.UnitTests.Mapping;

public sealed class DiffLogicTests
{
    [Fact]
    public void OrderChanges_SortsByKeyThenLabel()
    {
        var changes = new[]
        {
            new ChangeSummary { Key = "b", Label = "z", Action = "add" },
            new ChangeSummary { Key = "a", Label = "b", Action = "add" },
            new ChangeSummary { Key = "a", Label = "a", Action = "remove" }
        };

        var ordered = DeterministicOrdering.OrderChanges(changes).ToArray();

        Assert.Equal("a", ordered[0].Key);
        Assert.Equal("a", ordered[0].Label);
        Assert.Equal("a", ordered[1].Key);
        Assert.Equal("b", ordered[1].Label);
        Assert.Equal("b", ordered[2].Key);
    }
}
