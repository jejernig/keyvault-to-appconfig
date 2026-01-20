using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Services.Mapping;

namespace KeyVaultToAppConfig.UnitTests.Mapping;

public sealed class MappingEdgeCaseTests
{
    [Fact]
    public void Validate_UnsupportedTransform_ReturnsError()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """
        {
          "name": "Spec",
          "version": "v1",
          "rules": [
            {
              "ruleId": "r1",
              "strategyType": "direct",
              "sourceSelector": "A",
              "targetKey": "a",
              "priority": 1,
              "transforms": [
                { "transformType": "unknown" }
              ]
            }
          ]
        }
        """);

        try
        {
            var loader = new MappingSpecLoader();
            using var document = loader.Load(path);
            var validator = new MappingSpecValidator();
            var result = validator.Validate(document);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Field == "rules[0].transforms[0].transformType");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ExecuteAsync_TieUsesFirstRuleInSpecOrder()
    {
        var specification = new MappingSpecification
        {
            Name = "Spec",
            Version = "v1",
            Rules =
            {
                new MappingRule
                {
                    RuleId = "first",
                    StrategyType = MappingStrategyType.Regex,
                    SourceSelector = ".*",
                    TargetKey = "first",
                    Priority = 5
                },
                new MappingRule
                {
                    RuleId = "second",
                    StrategyType = MappingStrategyType.Regex,
                    SourceSelector = ".*",
                    TargetKey = "second",
                    Priority = 5
                }
            }
        };

        var engine = new MappingEngine();
        var run = await engine.ExecuteAsync(specification, new List<string> { "key" }, CancellationToken.None);

        Assert.True(run.NormalizedKeys.ContainsKey("first"));
        Assert.False(run.NormalizedKeys.ContainsKey("second"));
    }
}
