using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Services.Mapping;

namespace KeyVaultToAppConfig.UnitTests.Mapping;

public sealed class MappingEngineTests
{
    [Fact]
    public async Task ExecuteAsync_AppliesTransformsInOrder()
    {
        var specification = new MappingSpecification
        {
            Name = "Spec",
            Version = "v1",
            Rules =
            {
                new MappingRule
                {
                    RuleId = "r1",
                    StrategyType = MappingStrategyType.Direct,
                    SourceSelector = "Key",
                    TargetKey = "Value",
                    Priority = 1,
                    Transforms =
                    {
                        new TransformOperation { TransformType = TransformType.Lower },
                        new TransformOperation
                        {
                            TransformType = TransformType.Suffix,
                            Parameters = new Dictionary<string, string> { ["value"] = ".suffix" }
                        }
                    }
                }
            }
        };

        var engine = new MappingEngine();
        var run = await engine.ExecuteAsync(specification, new List<string> { "Key" }, CancellationToken.None);

        Assert.True(run.NormalizedKeys.ContainsKey("value.suffix"));
    }

    [Fact]
    public async Task ExecuteAsync_UsesHighestPriorityRule()
    {
        var specification = new MappingSpecification
        {
            Name = "Spec",
            Version = "v1",
            Rules =
            {
                new MappingRule
                {
                    RuleId = "low",
                    StrategyType = MappingStrategyType.Direct,
                    SourceSelector = "Key",
                    TargetKey = "low",
                    Priority = 1
                },
                new MappingRule
                {
                    RuleId = "high",
                    StrategyType = MappingStrategyType.Direct,
                    SourceSelector = "Key",
                    TargetKey = "high",
                    Priority = 100
                }
            }
        };

        var engine = new MappingEngine();
        var run = await engine.ExecuteAsync(specification, new List<string> { "Key" }, CancellationToken.None);

        Assert.True(run.NormalizedKeys.ContainsKey("high"));
    }

    [Fact]
    public async Task ExecuteAsync_PreservesSourceOrder()
    {
        var specification = new MappingSpecification
        {
            Name = "Spec",
            Version = "v1",
            Rules =
            {
                new MappingRule
                {
                    RuleId = "r1",
                    StrategyType = MappingStrategyType.Direct,
                    SourceSelector = "A",
                    TargetKey = "a",
                    Priority = 1
                },
                new MappingRule
                {
                    RuleId = "r2",
                    StrategyType = MappingStrategyType.Direct,
                    SourceSelector = "B",
                    TargetKey = "b",
                    Priority = 1
                }
            }
        };

        var engine = new MappingEngine();
        var run = await engine.ExecuteAsync(specification, new List<string> { "B", "A" }, CancellationToken.None);

        Assert.Equal(new[] { "b", "a" }, run.NormalizedKeys.Keys.ToArray());
    }

    [Fact]
    public async Task ExecuteAsync_ErrorPolicyFailsRun()
    {
        var specification = new MappingSpecification
        {
            Name = "Spec",
            Version = "v1",
            CollisionPolicy = CollisionPolicy.Error,
            Rules =
            {
                new MappingRule
                {
                    RuleId = "r1",
                    StrategyType = MappingStrategyType.Regex,
                    SourceSelector = ".*",
                    TargetKey = "same",
                    Priority = 1
                }
            }
        };

        var engine = new MappingEngine();
        var run = await engine.ExecuteAsync(specification, new List<string> { "A", "B" }, CancellationToken.None);

        Assert.Equal(MappingRunStatus.Failed, run.Status);
        Assert.NotNull(run.CollisionReport);
    }
}
