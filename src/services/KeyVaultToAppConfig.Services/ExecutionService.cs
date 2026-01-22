using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Auth;
using KeyVaultToAppConfig.Core.Errors;
using KeyVaultToAppConfig.Core.Enumeration;
using KeyVaultToAppConfig.Core.Observability;
using KeyVaultToAppConfig.Core.Writes;
using KeyVaultToAppConfig.Services.Errors;
using KeyVaultToAppConfig.Services.Observability;

namespace KeyVaultToAppConfig.Services;

public sealed class ExecutionService
{
    public async Task<RunReport> ExecuteAsync(RunConfiguration config, CancellationToken cancellationToken)
    {
        var correlationProvider = new CorrelationIdProvider();
        var correlationId = correlationProvider.GetOrCreate(config.CorrelationId);
        var logWriter = new StructuredLogWriter();
        var classifier = new ErrorClassifier();
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

            var errorRecord = CreateErrorRecord(
                classifier.Classify("run", new InvalidOperationException("validation")),
                "run",
                "validation",
                "Validation failed.");

            var report = new RunReport
            {
                RunId = correlationId,
                ExecutionMode = config.ExecutionMode,
                Timestamp = DateTimeOffset.UtcNow,
                Outcome = RunOutcome.FatalFailure,
                ExitCode = ObservabilityExitCodes.MapFromOutcome(RunOutcome.FatalFailure),
                Totals = new Totals { Failed = 1 },
                ErrorTotals = new ErrorTotals(),
                Failures =
                {
                    new FailureSummary
                    {
                        Key = "auth",
                        ErrorType = "static-secret",
                        Message = violations[0]
                    }
                },
                Errors = { errorRecord }
            };

            return report;
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

            var errorRecord = CreateErrorRecord(
                classifier.Classify("run", new InvalidOperationException(authResult.ErrorMessage)),
                "run",
                "auth",
                "Authentication failed.");

            var report = new RunReport
            {
                RunId = correlationId,
                ExecutionMode = config.ExecutionMode,
                Timestamp = DateTimeOffset.UtcNow,
                Outcome = RunOutcome.FatalFailure,
                ExitCode = ObservabilityExitCodes.MapFromOutcome(RunOutcome.FatalFailure),
                Totals = new Totals { Failed = 1 },
                ErrorTotals = new ErrorTotals(),
                Failures =
                {
                    new FailureSummary
                    {
                        Key = "auth",
                        ErrorType = authResult.ErrorCategory.ToString().ToLowerInvariant(),
                        Message = authResult.ErrorMessage
                    }
                },
                Errors = { errorRecord }
            };

            return report;
        }

        try
        {
            var credential = new DefaultAzureCredential();
            var secretClient = new SecretClient(new Uri(config.KeyVaultUri), credential);
            var enumerator = new KeyVaultSecretEnumerator(secretClient);
            var filter = EnumerationFilterBuilder.Build(config);
            var versionSelection = VersionSelectionBuilder.Build(config);
            var retryPolicy = new RetryPolicy();
            var secrets = await EnumerateWithRetryAsync(
                enumerator,
                filter,
                versionSelection,
                config.PageSize,
                config.ContinuationToken,
                retryPolicy,
                cancellationToken);

            var runner = new SecretOperationRunner(classifier);
            var operationResult = await runner.ExecuteAsync(
                secrets,
                config.FailFast,
                (_, _) => Task.CompletedTask,
                cancellationToken);

            var successfulCount = operationResult.Outcomes.Count(o => o.Status == SecretOutcomeStatus.Success);
            var recoverableCount = operationResult.Outcomes.Count(o => o.Status == SecretOutcomeStatus.RecoverableFailure);
            var unprocessedCount = operationResult.Outcomes.Count(o => o.Status == SecretOutcomeStatus.Unprocessed);
            var outcome = operationResult.WasCanceled
                ? RunOutcome.Canceled
                : recoverableCount > 0
                    ? RunOutcome.RecoverableFailures
                    : RunOutcome.Success;

            return new RunReport
            {
                RunId = correlationId,
                ExecutionMode = config.ExecutionMode,
                Timestamp = DateTimeOffset.UtcNow,
                Outcome = outcome,
                ExitCode = ObservabilityExitCodes.MapFromOutcome(outcome),
                Totals = new Totals
                {
                    Scanned = secrets.Count,
                    Failed = recoverableCount
                },
                ErrorTotals = new ErrorTotals
                {
                    SuccessfulSecrets = successfulCount,
                    RecoverableFailures = recoverableCount,
                    UnprocessedSecrets = unprocessedCount
                },
                SecretOutcomes = operationResult.Outcomes,
                Errors = operationResult.Errors,
                EnumeratedSecrets = secrets.ToList()
            };
        }
        catch (OperationCanceledException)
        {
            return new RunReport
            {
                RunId = correlationId,
                ExecutionMode = config.ExecutionMode,
                Timestamp = DateTimeOffset.UtcNow,
                Outcome = RunOutcome.Canceled,
                ExitCode = ObservabilityExitCodes.MapFromOutcome(RunOutcome.Canceled),
                Totals = new Totals(),
                ErrorTotals = new ErrorTotals()
            };
        }
        catch (Exception ex)
        {
            Log(logWriter, correlationId, "run-failed", "error", "Run failed with exception.", new Dictionary<string, string>
            {
                ["errorType"] = "fatal"
            });

            var errorRecord = CreateErrorRecord(
                classifier.Classify("run", ex),
                "run",
                "enumeration",
                ex.Message);

            return new RunReport
            {
                RunId = correlationId,
                ExecutionMode = config.ExecutionMode,
                Timestamp = DateTimeOffset.UtcNow,
                Outcome = RunOutcome.FatalFailure,
                ExitCode = ObservabilityExitCodes.MapFromOutcome(RunOutcome.FatalFailure),
                Totals = new Totals { Failed = 1 },
                ErrorTotals = new ErrorTotals(),
                Failures =
                {
                    new FailureSummary
                    {
                        Key = "enumeration",
                        ErrorType = "fatal",
                        Message = ex.Message
                    }
                },
                Errors = { errorRecord }
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

    private static ErrorRecord CreateErrorRecord(
        ErrorClassification classification,
        string scope,
        string stage,
        string summary)
    {
        return new ErrorRecord
        {
            ErrorId = Guid.NewGuid().ToString("N"),
            Classification = classification,
            Scope = scope,
            Stage = stage,
            Summary = summary,
            OccurredAt = DateTimeOffset.UtcNow
        };
    }

    private static async Task<IReadOnlyList<SecretDescriptor>> EnumerateWithRetryAsync(
        KeyVaultSecretEnumerator enumerator,
        EnumerationFilter filter,
        VersionSelection versionSelection,
        int? pageSize,
        string? continuationToken,
        RetryPolicy retryPolicy,
        CancellationToken cancellationToken)
    {
        var attempts = 0;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= retryPolicy.MaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempts = attempt;

            try
            {
                return await enumerator.EnumerateAsync(
                    filter,
                    versionSelection,
                    pageSize,
                    continuationToken,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt == retryPolicy.MaxAttempts)
                {
                    break;
                }

                var delaySeconds = Math.Min(
                    retryPolicy.MaxDelaySeconds,
                    retryPolicy.BaseDelaySeconds * (int)Math.Pow(2, attempt - 1));
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Enumeration failed after {attempts} attempts.",
            lastException);
    }
}
