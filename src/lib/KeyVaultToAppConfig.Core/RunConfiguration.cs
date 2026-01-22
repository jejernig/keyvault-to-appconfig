namespace KeyVaultToAppConfig.Core;

public sealed class RunConfiguration
{
    public required string KeyVaultUri { get; init; }
    public required string AppConfigEndpoint { get; init; }
    public required ExecutionMode ExecutionMode { get; init; }
    public string Environment { get; init; } = string.Empty;
    public string? MappingFile { get; init; }
    public int Parallelism { get; init; } = 1;
    public string? IncludePrefix { get; init; }
    public string? ExcludeRegex { get; init; }
    public string? OnlyTag { get; init; }
    public bool EnabledOnly { get; init; }
    public string VersionMode { get; init; } = "latest";
    public string? VersionMapPath { get; init; }
    public int? PageSize { get; init; }
    public string? ContinuationToken { get; init; }
    public string? ReportJson { get; init; }
    public string Mode { get; init; } = "kvref";
    public bool ConfirmCopyValue { get; init; }
    public Observability.VerbosityLevel Verbosity { get; init; } = Observability.VerbosityLevel.Normal;
    public string? CorrelationId { get; init; }
    public bool FailFast { get; init; }
    public bool DisableManagedIdentity { get; init; }
    public bool DisableWorkloadIdentity { get; init; }
    public bool DisableAzureCli { get; init; }
    public bool DisableVisualStudio { get; init; }
    public bool DisableVisualStudioCode { get; init; }
    public bool DisableSharedTokenCache { get; init; }
}
