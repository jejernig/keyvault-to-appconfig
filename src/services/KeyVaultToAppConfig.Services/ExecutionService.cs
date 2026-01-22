using Azure;
using Azure.Core;
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

        var resolver = new AuthResolver(diagnostics, config);
        var authResult = await resolver.ResolveAsync(new[]
        {
            "https://vault.azure.net/.default",
            "https://azconfig.io/.default"
        }, cancellationToken);

        if (!authResult.IsSuccess)
        {
            var errorData = new Dictionary<string, string>
            {
                ["errorType"] = authResult.ErrorCategory.ToString().ToLowerInvariant()
            };

            if (!string.IsNullOrWhiteSpace(authResult.ErrorDetail))
            {
                errorData["detail"] = authResult.ErrorDetail;
            }

            Log(logWriter, correlationId, "run-failed", "error", "Authentication failed.", errorData);

            var errorRecord = CreateErrorRecord(
                classifier.Classify("run", new InvalidOperationException(authResult.ErrorDetail)),
                "run",
                "auth",
                string.IsNullOrWhiteSpace(authResult.ErrorDetail)
                    ? "Authentication failed."
                    : authResult.ErrorDetail);

            var failureMessage = authResult.ErrorMessage;
            if (!string.IsNullOrWhiteSpace(authResult.ErrorDetail))
            {
                failureMessage = $"{failureMessage} {authResult.ErrorDetail}";
            }

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
                        Message = failureMessage
                    }
                },
                Errors = { errorRecord }
            };

            return report;
        }

        try
        {
            var credential = CreateTokenCredential(config);
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
            var detail = FormatExceptionSummary(ex);
            Log(logWriter, correlationId, "run-failed", "error", "Run failed with exception.", new Dictionary<string, string>
            {
                ["errorType"] = "fatal"
            });

            var errorRecord = CreateErrorRecord(
                classifier.Classify("run", ex),
                "run",
                "enumeration",
                detail);

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
                        Message = detail
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
            $"Enumeration failed after {attempts} attempts. {FormatExceptionSummary(lastException)}",
            lastException);
    }

    private static TokenCredential CreateTokenCredential(RunConfiguration config)
    {
        var options = new DefaultAzureCredentialOptions
        {
            ExcludeManagedIdentityCredential = config.DisableManagedIdentity,
            ExcludeWorkloadIdentityCredential = config.DisableWorkloadIdentity,
            ExcludeAzureCliCredential = config.DisableAzureCli,
            ExcludeVisualStudioCredential = config.DisableVisualStudio,
            ExcludeVisualStudioCodeCredential = config.DisableVisualStudioCode,
            ExcludeSharedTokenCacheCredential = config.DisableSharedTokenCache
        };

        return new DefaultAzureCredential(options);
    }

    private static string FormatExceptionSummary(Exception? exception)
    {
        if (exception is null)
        {
            return "No additional error detail available.";
        }

        if (exception is RequestFailedException requestFailed)
        {
            return $"{requestFailed.GetType().Name}: {requestFailed.Message} (Status: {requestFailed.Status}, Code: {requestFailed.ErrorCode})";
        }

        if (exception.InnerException is RequestFailedException innerRequestFailed)
        {
            return $"{exception.GetType().Name}: {exception.Message} | Inner: {innerRequestFailed.GetType().Name}: {innerRequestFailed.Message} (Status: {innerRequestFailed.Status}, Code: {innerRequestFailed.ErrorCode})";
        }

        return $"{exception.GetType().Name}: {exception.Message}";
    }
}
