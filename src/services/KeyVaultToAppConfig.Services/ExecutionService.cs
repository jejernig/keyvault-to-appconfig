using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Auth;
using KeyVaultToAppConfig.Core.Enumeration;

namespace KeyVaultToAppConfig.Services;

public sealed class ExecutionService
{
    public async Task<RunReport> ExecuteAsync(RunConfiguration config, CancellationToken cancellationToken)
    {
        var diagnostics = new AuthDiagnostics();
        var violations = diagnostics.ValidateNoStaticSecrets(new[]
        {
            config.KeyVaultUri,
            config.AppConfigEndpoint,
            config.MappingFile,
            config.IncludePrefix,
            config.ExcludeRegex,
            config.OnlyTag
        });

        if (violations.Count > 0)
        {
            return new RunReport
            {
                RunId = Guid.NewGuid().ToString("N"),
                ExecutionMode = config.ExecutionMode,
                Timestamp = DateTimeOffset.UtcNow,
                Totals = new Totals { Failed = 1 },
                Failures =
                {
                    new FailureSummary
                    {
                        Key = "auth",
                        ErrorType = "static-secret",
                        Message = violations[0]
                    }
                }
            };
        }

        var resolver = new AuthResolver(diagnostics);
        var authResult = await resolver.ResolveAsync(new[]
        {
            "https://vault.azure.net/.default",
            "https://azconfig.io/.default"
        }, cancellationToken);

        if (!authResult.IsSuccess)
        {
            return new RunReport
            {
                RunId = Guid.NewGuid().ToString("N"),
                ExecutionMode = config.ExecutionMode,
                Timestamp = DateTimeOffset.UtcNow,
                Totals = new Totals { Failed = 1 },
                Failures =
                {
                    new FailureSummary
                    {
                        Key = "auth",
                        ErrorType = authResult.ErrorCategory.ToString().ToLowerInvariant(),
                        Message = authResult.ErrorMessage
                    }
                }
            };
        }

        try
        {
            var credential = new DefaultAzureCredential();
            var secretClient = new SecretClient(new Uri(config.KeyVaultUri), credential);
            var enumerator = new KeyVaultSecretEnumerator(secretClient);
            var filter = EnumerationFilterBuilder.Build(config);
            var versionSelection = VersionSelectionBuilder.Build(config);
            var secrets = await enumerator.EnumerateAsync(
                filter,
                versionSelection,
                config.PageSize,
                config.ContinuationToken,
                cancellationToken);

            return new RunReport
            {
                RunId = Guid.NewGuid().ToString("N"),
                ExecutionMode = config.ExecutionMode,
                Timestamp = DateTimeOffset.UtcNow,
                Totals = new Totals
                {
                    Scanned = secrets.Count
                },
                EnumeratedSecrets = secrets.ToList()
            };
        }
        catch (Exception ex)
        {
            return new RunReport
            {
                RunId = Guid.NewGuid().ToString("N"),
                ExecutionMode = config.ExecutionMode,
                Timestamp = DateTimeOffset.UtcNow,
                Totals = new Totals { Failed = 1 },
                Failures =
                {
                    new FailureSummary
                    {
                        Key = "enumeration",
                        ErrorType = "fatal",
                        Message = ex.Message
                    }
                }
            };
        }
    }
}
