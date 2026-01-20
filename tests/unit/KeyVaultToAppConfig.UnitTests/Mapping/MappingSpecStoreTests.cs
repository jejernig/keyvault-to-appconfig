using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Services.Mapping;

namespace KeyVaultToAppConfig.UnitTests.Mapping;

public sealed class MappingSpecStoreTests
{
    [Fact]
    public async Task SaveAndLoad_RoundTripsSpecification()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var specification = new MappingSpecification
            {
                Name = "StoreSpec",
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
                    }
                }
            };

            var store = new MappingSpecStore(tempDir);
            await store.SaveAsync(specification, CancellationToken.None);

            var loaded = await store.LoadAsync("StoreSpec", "v1", CancellationToken.None);

            Assert.NotNull(loaded);
            Assert.Equal("StoreSpec", loaded?.Name);
            Assert.Equal("v1", loaded?.Version);
            var rules = loaded is null ? new List<MappingRule>() : loaded.Rules;
            Assert.Single(rules);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
