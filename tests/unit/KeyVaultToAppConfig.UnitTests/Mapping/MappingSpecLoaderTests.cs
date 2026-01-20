using KeyVaultToAppConfig.Core.Mapping;

namespace KeyVaultToAppConfig.UnitTests.Mapping;

public sealed class MappingSpecLoaderTests
{
    [Fact]
    public void Load_JsonSpec_ReturnsSpecification()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """
        {
          "name": "Sample",
          "version": "v1",
          "rules": [
            {
              "ruleId": "r1",
              "strategyType": "direct",
              "sourceSelector": "A",
              "targetKey": "a",
              "priority": 1
            }
          ]
        }
        """);

        try
        {
            var loader = new MappingSpecLoader();
            using var document = loader.Load(path);

            Assert.Equal("Sample", document.Specification.Name);
            Assert.Equal("v1", document.Specification.Version);
            Assert.Single(document.Specification.Rules);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_YamlSpec_ReturnsSpecification()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.yaml");
        File.WriteAllText(path, """
        name: SampleYaml
        version: v2
        rules:
          - ruleId: r1
            strategyType: direct
            sourceSelector: Alpha
            targetKey: alpha
            priority: 10
        """);

        try
        {
            var loader = new MappingSpecLoader();
            using var document = loader.Load(path);

            Assert.Equal("SampleYaml", document.Specification.Name);
            Assert.Equal("v2", document.Specification.Version);
            Assert.Single(document.Specification.Rules);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
