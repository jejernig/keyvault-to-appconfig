using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Auth;
using KeyVaultToAppConfig.Core.Enumeration;
using KeyVaultToAppConfig.Core.Observability;
using KeyVaultToAppConfig.Services.Observability;

namespace KeyVaultToAppConfig.Services;

public sealed class ExecutionService
{
    public async Task<RunReport> ExecuteAsync(RunConfiguration config, CancellationToken cancellationToken)
    {
        var correlationProvider = new CorrelationIdProvider();
        var correlationId = correlationProvider.GetOrCreate(config.CorrelationId);
        var logWriter = new StructuredLogWriter();
        Log(logWriter, correlationId, "run-start", "info", "Run started.", new Dictionary<string, string>
        {
            ["mode"] = config.ExecutionMode.ToString().ToLowerInvariant()
        });

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
            Log(logWriter, correlationId, "run-failed", "error", "Run blocked due to validation errors.", new Dictionary<string, string>
            {
                ["errorType"] = "static-secret"
            });

            return new RunReport
            {
                RunId = correlationId,
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
            Log(logWriter, correlationId, "run-failed", "error", "Authentication failed.", new Dictionary<string, string>
            {
                ["errorType"] = authResult.ErrorCategory.ToString().ToLowerInvariant()
            });

            return new RunReport
            {
                RunId = correlationId,
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
                RunId = correlationId,
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
            Log(logWriter, correlationId, "run-failed", "error", "Run failed with exception.", new Dictionary<string, string>
            {
                ["errorType"] = "fatal"
            });

            return new RunReport
            {
                RunId = correlationId,
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

    private static void Log(
        StructuredLogWriter writer,
        string correlationId,
        string eventName,
        string level,
        string message,
        IDictionary<string, string> data)
    {
        writer.Write(new LogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Level = level,
            Event = eventName,
            CorrelationId = correlationId,
            Message = message,
            Data = data
        });
    }
}
