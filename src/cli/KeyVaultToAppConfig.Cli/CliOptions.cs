namespace KeyVaultToAppConfig.Cli;

public sealed class CliOptions
{
    public string? KeyVaultUri { get; set; }
    public string? AppConfigEndpoint { get; set; }
    public bool DryRun { get; set; }
    public bool Diff { get; set; }
    public bool Apply { get; set; }
    public string Mode { get; set; } = "kvref";
    public bool ConfirmCopyValue { get; set; }
    public string? Environment { get; set; }
    public string? MappingFile { get; set; }
    public int? Parallelism { get; set; }
    public string? IncludePrefix { get; set; }
    public string? ExcludeRegex { get; set; }
    public string? OnlyTag { get; set; }
    public bool EnabledOnly { get; set; }
    public string? VersionMode { get; set; }
    public string? VersionMapPath { get; set; }
    public int? PageSize { get; set; }
    public string? ContinuationToken { get; set; }
    public string? ReportJson { get; set; }
    public string? Verbosity { get; set; }
    public string? CorrelationId { get; set; }
    public bool FailFast { get; set; }
    public bool DisableManagedIdentity { get; set; }
    public bool DisableWorkloadIdentity { get; set; }
    public bool DisableAzureCli { get; set; }
    public bool DisableVisualStudio { get; set; }
    public bool DisableVisualStudioCode { get; set; }
    public bool DisableSharedTokenCache { get; set; }
}
