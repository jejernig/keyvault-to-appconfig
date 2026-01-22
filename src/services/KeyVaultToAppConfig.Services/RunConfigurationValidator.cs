using System.Linq;
using System.Text.RegularExpressions;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Observability;

namespace KeyVaultToAppConfig.Services;

public sealed class RunConfigurationValidator
{
    public ValidationResult Validate(RunConfigurationInput options)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(options.KeyVaultUri))
        {
            result.Errors.Add("--keyvault-uri is required.");
        }

        if (string.IsNullOrWhiteSpace(options.AppConfigEndpoint))
        {
            result.Errors.Add("--appconfig-endpoint is required.");
        }

        var modeFlags = new[] { options.DryRun, options.Diff, options.Apply }.Count(flag => flag);
        if (modeFlags > 1)
        {
            result.Errors.Add("Only one of --dry-run, --diff, or --apply may be specified.");
        }

        if (string.Equals(options.Mode, "copyvalue", StringComparison.OrdinalIgnoreCase)
            && !options.ConfirmCopyValue)
        {
            result.Errors.Add("--mode copyvalue requires --confirm-copy-value.");
        }

        if (options.Parallelism is not null && options.Parallelism < 1)
        {
            result.Errors.Add("--parallelism must be >= 1 when provided.");
        }

        if (!string.IsNullOrWhiteSpace(options.MappingFile)
            && !File.Exists(options.MappingFile))
        {
            result.Errors.Add("--mapping-file must point to an existing file.");
        }

        if (!string.IsNullOrWhiteSpace(options.ExcludeRegex))
        {
            try
            {
                _ = new Regex(options.ExcludeRegex);
            }
            catch (ArgumentException)
            {
                result.Errors.Add("--exclude-regex must be a valid regex.");
            }
        }

        if (!string.IsNullOrWhiteSpace(options.VersionMode)
            && !string.Equals(options.VersionMode, "latest", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.VersionMode, "explicit", StringComparison.OrdinalIgnoreCase))
        {
            result.Errors.Add("--version-mode must be either latest or explicit.");
        }

        if (string.Equals(options.VersionMode, "explicit", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(options.VersionMapPath))
            {
                result.Errors.Add("--version-mode explicit requires --version-map.");
            }
            else if (!File.Exists(options.VersionMapPath))
            {
                result.Errors.Add("--version-map must point to an existing file.");
            }
        }

        if (options.PageSize is not null && options.PageSize < 1)
        {
            result.Errors.Add("--page-size must be >= 1 when provided.");
        }

        if (!string.IsNullOrWhiteSpace(options.Verbosity)
            && !TryParseVerbosity(options.Verbosity, out _))
        {
            result.Errors.Add("--verbosity must be minimal, normal, or verbose.");
        }

        if (!string.IsNullOrWhiteSpace(options.OnlyTag)
            && !options.OnlyTag.Contains('=', StringComparison.Ordinal))
        {
            result.Errors.Add("--only-tag must be in key=value format.");
        }

        return result;
    }

    public RunConfiguration BuildConfiguration(RunConfigurationInput options)
    {
        return new RunConfiguration
        {
            KeyVaultUri = options.KeyVaultUri ?? string.Empty,
            AppConfigEndpoint = options.AppConfigEndpoint ?? string.Empty,
            ExecutionMode = ResolveExecutionMode(options),
            Environment = options.Environment ?? string.Empty,
            MappingFile = options.MappingFile,
            Parallelism = options.Parallelism ?? 1,
            IncludePrefix = options.IncludePrefix,
            ExcludeRegex = options.ExcludeRegex,
            OnlyTag = options.OnlyTag,
            EnabledOnly = options.EnabledOnly,
            VersionMode = options.VersionMode ?? "latest",
            VersionMapPath = options.VersionMapPath,
            PageSize = options.PageSize,
            ContinuationToken = options.ContinuationToken,
            ReportJson = options.ReportJson,
            Mode = options.Mode,
            ConfirmCopyValue = options.ConfirmCopyValue,
            Verbosity = ParseVerbosity(options.Verbosity),
            CorrelationId = options.CorrelationId,
            FailFast = options.FailFast
        };
    }

    private static VerbosityLevel ParseVerbosity(string? value)
    {
        return TryParseVerbosity(value, out var verbosity) ? verbosity : VerbosityLevel.Normal;
    }

    private static bool TryParseVerbosity(string? value, out VerbosityLevel verbosity)
    {
        if (string.Equals(value, "minimal", StringComparison.OrdinalIgnoreCase))
        {
            verbosity = VerbosityLevel.Minimal;
            return true;
        }

        if (string.Equals(value, "verbose", StringComparison.OrdinalIgnoreCase))
        {
            verbosity = VerbosityLevel.Verbose;
            return true;
        }

        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "normal", StringComparison.OrdinalIgnoreCase))
        {
            verbosity = VerbosityLevel.Normal;
            return true;
        }

        verbosity = VerbosityLevel.Normal;
        return false;
    }

    private static ExecutionMode ResolveExecutionMode(RunConfigurationInput options)
    {
        if (options.Apply)
        {
            return ExecutionMode.Apply;
        }

        if (options.Diff)
        {
            return ExecutionMode.Diff;
        }

        return ExecutionMode.DryRun;
    }
}
