using KeyVaultToAppConfig.Services.Secrets;

namespace KeyVaultToAppConfig.UnitTests.Secrets;

public sealed class SecretUriResolverTests
{
    [Fact]
    public void Resolve_ReturnsLatestVersionWhenMissing()
    {
        var resolver = new SecretUriResolver();
        var result = resolver.Resolve("https://example.vault.azure.net/secrets/demo");

        Assert.True(result.IsValid);
        Assert.Equal("demo", result.SecretName);
        Assert.Equal("latest", result.Version);
        Assert.EndsWith("/latest", result.ResolvedUri);
    }

    [Fact]
    public void Resolve_ReturnsFailureForInvalidHost()
    {
        var resolver = new SecretUriResolver();
        var result = resolver.Resolve("https://example.com/secrets/demo");

        Assert.False(result.IsValid);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public void Resolve_KeepsVersionWhenProvided()
    {
        var resolver = new SecretUriResolver();
        var result = resolver.Resolve("https://example.vault.azure.net/secrets/demo/123");

        Assert.True(result.IsValid);
        Assert.Equal("123", result.Version);
        Assert.Equal("https://example.vault.azure.net/secrets/demo/123", result.ResolvedUri);
    }
}
