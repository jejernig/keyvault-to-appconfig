using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Auth;

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

        var report = new RunReport
        {
            RunId = Guid.NewGuid().ToString("N"),
            ExecutionMode = config.ExecutionMode,
            Timestamp = DateTimeOffset.UtcNow,
            Totals = new Totals()
        };

        if (config.ExecutionMode == ExecutionMode.Apply)
        {
            // Apply path placeholder; real implementation will populate changes and failures.
            report.Totals.Scanned = 0;
        }

        return report;
    }
}
