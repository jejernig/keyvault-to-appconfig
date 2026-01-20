using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.UnitTests;

public sealed class DiffOrderingTests
{
    [Fact]
    public void OrderChanges_SortsByKeyLabelAction()
    {
        var changes = new List<ChangeSummary>
        {
            new() { Key = "b", Label = "prod", Action = "update", Reason = "" },
            new() { Key = "a", Label = "prod", Action = "update", Reason = "" },
            new() { Key = "a", Label = "dev", Action = "create", Reason = "" },
            new() { Key = "a", Label = "dev", Action = "update", Reason = "" }
        };

        var ordered = DeterministicOrdering.OrderChanges(changes).ToList();

        Assert.Equal("a", ordered[0].Key);
        Assert.Equal("dev", ordered[0].Label);
        Assert.Equal("create", ordered[0].Action);
        Assert.Equal("a", ordered[1].Key);
        Assert.Equal("dev", ordered[1].Label);
        Assert.Equal("update", ordered[1].Action);
        Assert.Equal("a", ordered[2].Key);
        Assert.Equal("prod", ordered[2].Label);
        Assert.Equal("update", ordered[2].Action);
        Assert.Equal("b", ordered[3].Key);
    }
}
