using KeyVaultToAppConfig.Core.Enumeration;
using KeyVaultToAppConfig.Core.Errors;
using KeyVaultToAppConfig.Services.Errors;

namespace KeyVaultToAppConfig.UnitTests.ErrorHandling;

public sealed class PerSecretIsolationTests
{
    [Fact]
    public async Task ExecuteAsync_ContinuesAfterRecoverableFailure()
    {
        var secrets = new List<SecretDescriptor>
        {
            new() { Name = "one", Version = "v1", IsEnabled = true, LastUpdated = DateTimeOffset.UtcNow },
            new() { Name = "two", Version = "v1", IsEnabled = true, LastUpdated = DateTimeOffset.UtcNow }
        };

        var runner = new SecretOperationRunner(new ErrorClassifier());
        var result = await runner.ExecuteAsync(
            secrets,
            failFast: false,
            (secret, _) =>
            {
                if (secret.Name == "one")
                {
                    throw new InvalidOperationException("boom");
                }

                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.Equal(2, result.Outcomes.Count);
        Assert.Equal(SecretOutcomeStatus.RecoverableFailure, result.Outcomes[0].Status);
        Assert.Equal(SecretOutcomeStatus.Success, result.Outcomes[1].Status);
        Assert.Single(result.Errors);
    }
}
