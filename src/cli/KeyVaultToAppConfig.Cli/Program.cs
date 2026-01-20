using System.CommandLine;
using System.CommandLine.Invocation;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Services;

namespace KeyVaultToAppConfig.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var keyvaultUriOption = new Option<string>("--keyvault-uri") { IsRequired = true };
        var appconfigEndpointOption = new Option<string>("--appconfig-endpoint") { IsRequired = true };
        var dryRunOption = new Option<bool>("--dry-run");
        var diffOption = new Option<bool>("--diff");
        var applyOption = new Option<bool>("--apply");
        var modeOption = new Option<string>("--mode", () => "kvref");
        var confirmCopyValueOption = new Option<bool>("--confirm-copy-value");
        var environmentOption = new Option<string?>("--environment");
        var mappingFileOption = new Option<string?>("--mapping-file");
        var parallelismOption = new Option<int?>("--parallelism");
        var includePrefixOption = new Option<string?>("--include-prefix");
        var excludeRegexOption = new Option<string?>("--exclude-regex");
        var onlyTagOption = new Option<string?>("--only-tag");
        var reportJsonOption = new Option<string?>("--report-json");

        var rootCommand = new RootCommand("Key Vault to App Configuration standardizer")
        {
            keyvaultUriOption,
            appconfigEndpointOption,
            dryRunOption,
            diffOption,
            applyOption,
            modeOption,
            confirmCopyValueOption,
            environmentOption,
            mappingFileOption,
            parallelismOption,
            includePrefixOption,
            excludeRegexOption,
            onlyTagOption,
            reportJsonOption
        };

        rootCommand.SetHandler(async context =>
        {
            var parse = context.ParseResult;
            var options = new CliOptions
            {
                KeyVaultUri = parse.GetValueForOption(keyvaultUriOption),
                AppConfigEndpoint = parse.GetValueForOption(appconfigEndpointOption),
                DryRun = parse.GetValueForOption(dryRunOption),
                Diff = parse.GetValueForOption(diffOption),
                Apply = parse.GetValueForOption(applyOption),
                Mode = parse.GetValueForOption(modeOption) ?? "kvref",
                ConfirmCopyValue = parse.GetValueForOption(confirmCopyValueOption),
                Environment = parse.GetValueForOption(environmentOption),
                MappingFile = parse.GetValueForOption(mappingFileOption),
                Parallelism = parse.GetValueForOption(parallelismOption),
                IncludePrefix = parse.GetValueForOption(includePrefixOption),
                ExcludeRegex = parse.GetValueForOption(excludeRegexOption),
                OnlyTag = parse.GetValueForOption(onlyTagOption),
                ReportJson = parse.GetValueForOption(reportJsonOption)
            };

            var exitCode = await RunAsync(options, context.GetCancellationToken());
            context.ExitCode = exitCode;
        });

        return await rootCommand.InvokeAsync(args);
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
