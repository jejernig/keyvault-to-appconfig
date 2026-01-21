using System.CommandLine;
using KeyVaultToAppConfig.Cli.Commands;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Services;

namespace KeyVaultToAppConfig.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var keyvaultUriOption = new Option<string>("--keyvault-uri") { Required = true };
        var appconfigEndpointOption = new Option<string>("--appconfig-endpoint") { Required = true };
        var dryRunOption = new Option<bool>("--dry-run");
        var diffOption = new Option<bool>("--diff");
        var applyOption = new Option<bool>("--apply");
        var modeOption = new Option<string>("--mode") { DefaultValueFactory = _ => "kvref" };
        var confirmCopyValueOption = new Option<bool>("--confirm-copy-value");
        var environmentOption = new Option<string?>("--environment");
        var mappingFileOption = new Option<string?>("--mapping-file");
        var parallelismOption = new Option<int?>("--parallelism");
        var includePrefixOption = new Option<string?>("--include-prefix");
        var excludeRegexOption = new Option<string?>("--exclude-regex");
        var onlyTagOption = new Option<string?>("--only-tag");
        var enabledOnlyOption = new Option<bool>("--enabled-only");
        var versionModeOption = new Option<string?>("--version-mode") { DefaultValueFactory = _ => "latest" };
        var versionMapOption = new Option<string?>("--version-map");
        var pageSizeOption = new Option<int?>("--page-size");
        var continuationTokenOption = new Option<string?>("--continuation-token");
        var reportJsonOption = new Option<string?>("--report-json");

        var rootCommand = new RootCommand("Key Vault to App Configuration standardizer");
        rootCommand.Add(keyvaultUriOption);
        rootCommand.Add(appconfigEndpointOption);
        rootCommand.Add(dryRunOption);
        rootCommand.Add(diffOption);
        rootCommand.Add(applyOption);
        rootCommand.Add(modeOption);
        rootCommand.Add(confirmCopyValueOption);
        rootCommand.Add(environmentOption);
        rootCommand.Add(mappingFileOption);
        rootCommand.Add(parallelismOption);
        rootCommand.Add(includePrefixOption);
        rootCommand.Add(excludeRegexOption);
        rootCommand.Add(onlyTagOption);
        rootCommand.Add(enabledOnlyOption);
        rootCommand.Add(versionModeOption);
        rootCommand.Add(versionMapOption);
        rootCommand.Add(pageSizeOption);
        rootCommand.Add(continuationTokenOption);
        rootCommand.Add(reportJsonOption);

        rootCommand.Add(MappingSpecCommand.Build());
        rootCommand.Add(MappingValidateCommand.Build());
        rootCommand.Add(MappingRunCommand.Build());
        rootCommand.Add(PlanCommand.Build());
        rootCommand.Add(DiffCommand.Build());
        rootCommand.Add(DryRunCommand.Build());
        rootCommand.Add(ApplyCommand.Build());

        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var options = new CliOptions
            {
                KeyVaultUri = parseResult.GetRequiredValue(keyvaultUriOption),
                AppConfigEndpoint = parseResult.GetRequiredValue(appconfigEndpointOption),
                DryRun = parseResult.GetValue(dryRunOption),
                Diff = parseResult.GetValue(diffOption),
                Apply = parseResult.GetValue(applyOption),
                Mode = parseResult.GetValue(modeOption) ?? "kvref",
                ConfirmCopyValue = parseResult.GetValue(confirmCopyValueOption),
                Environment = parseResult.GetValue(environmentOption),
                MappingFile = parseResult.GetValue(mappingFileOption),
                Parallelism = parseResult.GetValue(parallelismOption),
                IncludePrefix = parseResult.GetValue(includePrefixOption),
                ExcludeRegex = parseResult.GetValue(excludeRegexOption),
                OnlyTag = parseResult.GetValue(onlyTagOption),
                EnabledOnly = parseResult.GetValue(enabledOnlyOption),
                VersionMode = parseResult.GetValue(versionModeOption),
                VersionMapPath = parseResult.GetValue(versionMapOption),
                PageSize = parseResult.GetValue(pageSizeOption),
                ContinuationToken = parseResult.GetValue(continuationTokenOption),
                ReportJson = parseResult.GetValue(reportJsonOption)
            };

            return await RunAsync(options, cancellationToken);
        });

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }

    private static async Task<int> RunAsync(CliOptions options, CancellationToken cancellationToken)
    {
        var validator = new RunConfigurationValidator();
        var input = new RunConfigurationInput
        {
            KeyVaultUri = options.KeyVaultUri,
            AppConfigEndpoint = options.AppConfigEndpoint,
            DryRun = options.DryRun,
            Diff = options.Diff,
            Apply = options.Apply,
            Mode = options.Mode,
            ConfirmCopyValue = options.ConfirmCopyValue,
            Environment = options.Environment,
            MappingFile = options.MappingFile,
            Parallelism = options.Parallelism,
            IncludePrefix = options.IncludePrefix,
            ExcludeRegex = options.ExcludeRegex,
            OnlyTag = options.OnlyTag,
            EnabledOnly = options.EnabledOnly,
            VersionMode = options.VersionMode,
            VersionMapPath = options.VersionMapPath,
            PageSize = options.PageSize,
            ContinuationToken = options.ContinuationToken,
            ReportJson = options.ReportJson
        };

        var validation = validator.Validate(input);
        var output = new OutputWriter();

        if (!validation.IsValid)
        {
            output.WriteValidationErrors(validation.Errors);
            output.WriteUsage(HelpText.Usage);
            return ExitCodes.FatalError;
        }

        var config = validator.BuildConfiguration(input);
        var executionService = new ExecutionService();
        var report = await executionService.ExecuteAsync(config, cancellationToken);

        report.Changes = DeterministicOrdering.OrderChanges(report.Changes).ToList();
        output.WriteRunSummary(report);
        output.WriteFailures(report.Failures);
        output.WriteEnumeration(report.EnumeratedSecrets);

        if (config.ExecutionMode is ExecutionMode.Diff or ExecutionMode.DryRun)
        {
            output.WriteDiff(report.Changes);
        }

        if (!string.IsNullOrWhiteSpace(config.ReportJson))
        {
            var reportWriter = new ReportWriter();
            await reportWriter.WriteJsonAsync(report, config.ReportJson, cancellationToken);
        }

        if (report.Totals.Failed > 0)
        {
            return ExitCodes.PartialFailures;
        }

        if (report.Totals.Changed > 0)
        {
            return ExitCodes.SuccessWithChanges;
        }

        return ExitCodes.SuccessNoChanges;
    }
}
