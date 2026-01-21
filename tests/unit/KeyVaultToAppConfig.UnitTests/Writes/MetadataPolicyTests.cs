using KeyVaultToAppConfig.Core.Writes;
using KeyVaultToAppConfig.Services.Writes;

namespace KeyVaultToAppConfig.UnitTests.Writes;

public sealed class MetadataPolicyTests
{
    [Fact]
    public void ApplyManagedMetadata_PreservesUnmanagedTags()
    {
        var existing = new Dictionary<string, string>
        {
            ["owner"] = "team-a",
            ["custom"] = "keep"
        };

        var metadata = new ManagedMetadata
        {
            Source = "tooling",
            Timestamp = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        var policy = new MetadataPolicy();
        var merged = policy.ApplyManagedMetadata(existing, metadata);

        Assert.Equal("team-a", merged["owner"]);
        Assert.Equal("keep", merged["custom"]);
        Assert.Equal("tooling", merged["managedBy"]);
        Assert.Contains("managedAt", merged.Keys);
    }
}
