using Azure;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Auth;
using KeyVaultToAppConfig.Core.Errors;
using KeyVaultToAppConfig.Core.Enumeration;
using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Core.Observability;
using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Core.Writes;
using KeyVaultToAppConfig.Services.Errors;
using KeyVaultToAppConfig.Services.Mapping;
using KeyVaultToAppConfig.Services.Observability;
using KeyVaultToAppConfig.Services.Planning;
using KeyVaultToAppConfig.Services.Writes;

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
            if (operationResult.WasCanceled)
            {
                return new RunReport
                {
                    RunId = correlationId,
                    ExecutionMode = config.ExecutionMode,
                    Timestamp = DateTimeOffset.UtcNow,
                    Outcome = RunOutcome.Canceled,
                    ExitCode = ObservabilityExitCodes.MapFromOutcome(RunOutcome.Canceled),
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

            var sourceKeys = BuildSourceKeys(secrets);
            MappingRun mappingRun;
            try
            {
                mappingRun = await BuildMappingRunAsync(config.MappingFile, sourceKeys, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                Log(logWriter, correlationId, "run-failed", "error", "Mapping validation failed.", new Dictionary<string, string>
                {
                    ["errorType"] = "mapping"
                });

                var detail = FormatExceptionSummary(ex);
                var errorRecord = CreateErrorRecord(
                    classifier.Classify("run", ex),
                    "run",
                    "mapping",
                    detail);

                return new RunReport
                {
                    RunId = correlationId,
                    ExecutionMode = config.ExecutionMode,
                    Timestamp = DateTimeOffset.UtcNow,
                    Outcome = RunOutcome.FatalFailure,
                    ExitCode = ObservabilityExitCodes.MapFromOutcome(RunOutcome.FatalFailure),
                    Totals = new Totals { Failed = 1 },
                    ErrorTotals = new ErrorTotals
                    {
                        SuccessfulSecrets = successfulCount,
                        RecoverableFailures = recoverableCount,
                        UnprocessedSecrets = unprocessedCount
                    },
                    Failures =
                    {
                        new FailureSummary
                        {
                            Key = "mapping",
                            ErrorType = "validation",
                            Message = detail
                        }
                    },
                    SecretOutcomes = operationResult.Outcomes,
                    Errors = { errorRecord },
                    EnumeratedSecrets = secrets.ToList()
                };
            }
            if (mappingRun.Status == MappingRunStatus.Failed)
            {
                Log(logWriter, correlationId, "run-failed", "error", "Mapping failed.", new Dictionary<string, string>
                {
                    ["errorType"] = "mapping"
                });

                var report = new RunReport
                {
                    RunId = correlationId,
                    ExecutionMode = config.ExecutionMode,
                    Timestamp = DateTimeOffset.UtcNow,
                    Outcome = RunOutcome.FatalFailure,
                    ExitCode = ObservabilityExitCodes.MapFromOutcome(RunOutcome.FatalFailure),
                    Totals = new Totals { Failed = 1 },
                    ErrorTotals = new ErrorTotals
                    {
                        SuccessfulSecrets = successfulCount,
                        RecoverableFailures = recoverableCount,
                        UnprocessedSecrets = unprocessedCount
                    },
                    Failures =
                    {
                        new FailureSummary
                        {
                            Key = "mapping",
                            ErrorType = "mapping",
                            Message = "Mapping failed due to unmatched rules or collision policy."
                        }
                    },
                    SecretOutcomes = operationResult.Outcomes,
                    Errors = operationResult.Errors,
                    EnumeratedSecrets = secrets.ToList()
                };

                return report;
            }

            if (!IsReferenceMode(config.Mode))
            {
                Log(logWriter, correlationId, "run-failed", "error", "Copy-value mode is not supported in run execution.", new Dictionary<string, string>
                {
                    ["errorType"] = "mode"
                });

                var errorRecord = CreateErrorRecord(
                    classifier.Classify("run", new InvalidOperationException("copy-value")),
                    "run",
                    "mode",
                    "Copy-value mode is not supported in run execution.");

                return new RunReport
                {
                    RunId = correlationId,
                    ExecutionMode = config.ExecutionMode,
                    Timestamp = DateTimeOffset.UtcNow,
                    Outcome = RunOutcome.FatalFailure,
                    ExitCode = ObservabilityExitCodes.MapFromOutcome(RunOutcome.FatalFailure),
                    Totals = new Totals { Failed = 1 },
                    ErrorTotals = new ErrorTotals
                    {
                        SuccessfulSecrets = successfulCount,
                        RecoverableFailures = recoverableCount,
                        UnprocessedSecrets = unprocessedCount
                    },
                    Failures =
                    {
                        new FailureSummary
                        {
                            Key = "mode",
                            ErrorType = "unsupported",
                            Message = "Copy-value mode is not supported in run execution."
                        }
                    },
                    Errors = { errorRecord },
                    SecretOutcomes = operationResult.Outcomes,
                    EnumeratedSecrets = secrets.ToList()
                };
            }

            var labels = ParseEnvironmentLabels(config.Environment);
            var secretLookup = secrets.ToDictionary(secret => secret.Name, StringComparer.OrdinalIgnoreCase);
            var desiredBuilder = new DesiredStateBuilder();
            var desiredState = desiredBuilder.BuildFromMapping(
                mappingRun,
                labels,
                sourceKey =>
                {
                    if (!secretLookup.TryGetValue(sourceKey, out var secret))
                    {
                        throw new InvalidOperationException(
                            $"Mapped source key '{sourceKey}' was not found in enumerated secrets.");
                    }

                    var secretUri = BuildSecretUri(config.KeyVaultUri, secret);
                    return BuildKeyVaultReferenceValue(secretUri);
                },
                _ => KeyVaultReferenceContentType,
                "keyvault-to-appconfig");

            var configClient = new ConfigurationClient(new Uri(config.AppConfigEndpoint), credential);
            var existingReader = new ExistingStateReader(configClient);
            var existingState = await existingReader.ReadAsync(
                null,
                labels,
                config.PageSize,
                config.ContinuationToken,
                cancellationToken);

            var planningEngine = new PlanningEngine();
            var plan = await planningEngine.BuildPlanAsync(desiredState, existingState, cancellationToken);
            var changes = plan.DiffItems.Select(item => new ChangeSummary
            {
                Key = item.Key,
                Label = item.Label,
                Action = item.Classification.ToString().ToLowerInvariant(),
                Reason = item.Reason
            }).ToList();

            var failureSummaries = plan.Conflicts.Select(conflict => new FailureSummary
            {
                Key = conflict.Key,
                ErrorType = "conflict",
                Message = "Conflict detected for key/label."
            }).ToList();

            if (config.ExecutionMode == ExecutionMode.Apply)
            {
                var labelContext = new LabelContext
                {
                    EnvironmentLabel = labels.FirstOrDefault(),
                    UseEmptyLabelWhenMissing = true
                };

                var managedMetadata = new ManagedMetadata
                {
                    Source = "keyvault-to-appconfig",
                    Timestamp = DateTimeOffset.UtcNow
                };

                var writePlanner = new WritePlanner();
                var writePlan = await writePlanner.BuildPlanAsync(
                    desiredState,
                    existingState,
                    labelContext,
                    managedMetadata,
                    cancellationToken);

                var writeOptions = new WriteOptions
                {
                    MaxParallelism = Math.Max(1, config.Parallelism)
                };

                var writeExecutor = new WriteExecutor(configClient);
                var writeReport = await writeExecutor.ExecuteAsync(writePlan, writeOptions, cancellationToken);

                failureSummaries.AddRange(writeReport.Results
                    .Where(result => result.Status == WriteStatus.Failed)
                    .Select(result => new FailureSummary
                    {
                        Key = result.Key,
                        ErrorType = "write",
                        Message = result.FailureReason ?? "Write failed."
                    }));

                var pruneFailures = new List<FailureSummary>();
                var prunedChanges = new List<ChangeSummary>();
                var prunedCount = 0;

                if (config.Prune && config.ConfirmPrune)
                {
                    var deleteResults = await PruneAsync(
                        configClient,
                        existingState,
                        desiredState,
                        config.Parallelism,
                        cancellationToken);

                    prunedCount = deleteResults.PrunedCount;
                    prunedChanges = deleteResults.Changes;
                    pruneFailures = deleteResults.Failures;

                    failureSummaries.AddRange(pruneFailures);
                }

                var outcome = writeReport.Totals.FailedCount > 0 || plan.Conflicts.Count > 0
                    || pruneFailures.Count > 0
                    ? RunOutcome.RecoverableFailures
                    : RunOutcome.Success;

                if (prunedChanges.Count > 0)
                {
                    changes.AddRange(prunedChanges);
                }

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
                        Changed = writeReport.Totals.CreateCount + writeReport.Totals.UpdateCount + prunedCount,
                        Skipped = writeReport.Totals.SkipCount,
                        Failed = writeReport.Totals.FailedCount + pruneFailures.Count
                    },
                    ErrorTotals = new ErrorTotals
                    {
                        SuccessfulSecrets = successfulCount,
                        RecoverableFailures = recoverableCount,
                        UnprocessedSecrets = unprocessedCount
                    },
                    Changes = changes,
                    Failures = failureSummaries,
                    SecretOutcomes = operationResult.Outcomes,
                    Errors = operationResult.Errors,
                    EnumeratedSecrets = secrets.ToList()
                };
            }

            var planOutcome = plan.Conflicts.Count > 0
                ? RunOutcome.RecoverableFailures
                : RunOutcome.Success;

            return new RunReport
            {
                RunId = correlationId,
                ExecutionMode = config.ExecutionMode,
                Timestamp = DateTimeOffset.UtcNow,
                Outcome = planOutcome,
                ExitCode = ObservabilityExitCodes.MapFromOutcome(planOutcome),
                Totals = new Totals
                {
                    Scanned = secrets.Count,
                    Changed = plan.Totals.CreateCount + plan.Totals.UpdateCount,
                    Skipped = plan.Totals.UnchangedCount + plan.Totals.ConflictCount,
                    Failed = plan.Totals.ConflictCount
                },
                ErrorTotals = new ErrorTotals
                {
                    SuccessfulSecrets = successfulCount,
                    RecoverableFailures = recoverableCount,
                    UnprocessedSecrets = unprocessedCount
                },
                Changes = changes,
                Failures = failureSummaries,
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

    private static async Task<MappingRun> BuildMappingRunAsync(
        string? mappingPath,
        IReadOnlyList<string> sourceKeys,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(mappingPath))
        {
            return BuildPassThroughMappingRun(sourceKeys);
        }

        var loader = new MappingSpecLoader();
        using var document = loader.Load(mappingPath);
        var validator = new MappingSpecValidator();
        var validation = validator.Validate(document);
        if (!validation.IsValid)
        {
            var detail = string.Join(
                "; ",
                validation.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.Field)
                        ? error.Message
                        : $"{error.Field}: {error.Message}"));
            throw new InvalidOperationException($"Mapping spec validation failed: {detail}");
        }

        var engine = new MappingEngine();
        return await engine.ExecuteAsync(document.Specification, sourceKeys, cancellationToken);
    }

    private static MappingRun BuildPassThroughMappingRun(IReadOnlyList<string> sourceKeys)
    {
        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in sourceKeys)
        {
            normalized[key] = key;
        }

        return new MappingRun
        {
            SpecificationId = "pass-through",
            SpecificationVersion = "1.0",
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            Status = MappingRunStatus.Succeeded,
            NormalizedKeys = normalized
        };
    }

    private static IReadOnlyList<string> BuildSourceKeys(IEnumerable<SecretDescriptor> secrets)
    {
        return secrets
            .OrderBy(secret => secret.Name, StringComparer.OrdinalIgnoreCase)
            .Select(secret => secret.Name)
            .ToList();
    }

    private static bool IsReferenceMode(string mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return true;
        }

        return string.Equals(mode, "kvref", StringComparison.OrdinalIgnoreCase)
               || string.Equals(mode, "reference", StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> ParseEnvironmentLabels(string environment)
    {
        if (string.IsNullOrWhiteSpace(environment))
        {
            return new List<string>();
        }

        return environment
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(label => label.Trim())
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(label => label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string BuildSecretUri(string keyVaultUri, SecretDescriptor secret)
    {
        var baseUri = keyVaultUri.TrimEnd('/');
        return $"{baseUri}/secrets/{secret.Name}/{secret.Version}";
    }

    private static string BuildKey(string key, string label) => $"{key}::{label}";

    private static string BuildKeyVaultReferenceValue(string secretUri)
    {
        var payload = new KeyVaultReferenceValue { Uri = secretUri };
        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private const string KeyVaultReferenceContentType =
        "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private sealed class PruneResult
    {
        public int PrunedCount { get; set; }
        public List<ChangeSummary> Changes { get; set; } = new();
        public List<FailureSummary> Failures { get; set; } = new();
    }

    private static async Task<PruneResult> PruneAsync(
        ConfigurationClient client,
        ExistingStateSnapshot existingState,
        DesiredState desiredState,
        int parallelism,
        CancellationToken cancellationToken)
    {
        var result = new PruneResult();
        var desiredKeys = desiredState.Entries
            .Select(entry => BuildKey(entry.Key, entry.Label))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var entriesToDelete = existingState.Entries
            .Where(entry => !desiredKeys.Contains(BuildKey(entry.Key, entry.Label)))
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var semaphore = new SemaphoreSlim(Math.Max(1, parallelism));
        var tasks = entriesToDelete.Select(async entry =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await client.DeleteConfigurationSettingAsync(entry.Key, entry.Label, cancellationToken);
                lock (result)
                {
                    result.PrunedCount++;
                    result.Changes.Add(new ChangeSummary
                    {
                        Key = entry.Key,
                        Label = entry.Label,
                        Action = "delete",
                        Reason = "Pruned (not in desired state)"
                    });
                }
            }
            catch (Exception ex)
            {
                lock (result)
                {
                    result.Failures.Add(new FailureSummary
                    {
                        Key = entry.Key,
                        ErrorType = "prune",
                        Message = FormatExceptionSummary(ex)
                    });
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return result;
    }

    private sealed class KeyVaultReferenceValue
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
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
