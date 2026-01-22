using KeyVaultToAppConfig.Core.Enumeration;
using KeyVaultToAppConfig.Core.Errors;
using KeyVaultToAppConfig.Services.Errors;

namespace KeyVaultToAppConfig.UnitTests.ErrorHandling;

public sealed class CancellationHandlingTests
{
    [Fact]
    public async Task ExecuteAsync_MarksUnprocessedSecretsOnCancellation()
    {
        var secrets = new List<SecretDescriptor>
        {
            new() { Name = "one", Version = "v1", IsEnabled = true, LastUpdated = DateTimeOffset.UtcNow },
            new() { Name = "two", Version = "v1", IsEnabled = true, LastUpdated = DateTimeOffset.UtcNow }
        };

        var runner = new SecretOperationRunner(new ErrorClassifier());
        using var cts = new CancellationTokenSource();
        var calls = 0;

        var result = await runner.ExecuteAsync(
            secrets,
            failFast: false,
            (secret, _) =>
            {
                calls++;
                if (calls == 1)
                {
                    cts.Cancel();
                }

                return Task.CompletedTask;
            },
            cts.Token);

        Assert.True(result.WasCanceled);
        Assert.Equal(2, result.Outcomes.Count);
        Assert.Equal(SecretOutcomeStatus.Success, result.Outcomes[0].Status);
        Assert.Equal(SecretOutcomeStatus.Unprocessed, result.Outcomes[1].Status);
    }
}
