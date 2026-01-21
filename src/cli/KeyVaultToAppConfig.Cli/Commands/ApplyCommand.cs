using System.CommandLine;
using System.Text.Json;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Writes;
using KeyVaultToAppConfig.Services.Writes;

namespace KeyVaultToAppConfig.Cli.Commands;

public static class ApplyCommand
{
    public static Command Build()
    {
        var appConfigEndpointOption = new Option<string>("--appconfig-endpoint") { Required = true };
        var planPathOption = new Option<string>("--plan-path") { Required = true };
        var maxParallelismOption = new Option<int>("--max-parallelism") { DefaultValueFactory = _ => 4 };
        var retryMaxOption = new Option<int>("--retry-max") { DefaultValueFactory = _ => 3 };
        var retryBaseDelayOption = new Option<int>("--retry-base-delay") { DefaultValueFactory = _ => 1 };
        var retryMaxDelayOption = new Option<int>("--retry-max-delay") { DefaultValueFactory = _ => 10 };
        var rollbackOption = new Option<bool>("--rollback");
        var snapshotIdOption = new Option<string?>("--snapshot-id");
        var reportJsonOption = new Option<string?>("--report-json");

        var command = new Command("apply", "Apply a write plan to App Configuration")
        {
            appConfigEndpointOption,
            planPathOption,
            maxParallelismOption,
            retryMaxOption,
            retryBaseDelayOption,
            retryMaxDelayOption,
            rollbackOption,
            snapshotIdOption,
            reportJsonOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var endpoint = parseResult.GetRequiredValue(appConfigEndpointOption);
            var planPath = parseResult.GetRequiredValue(planPathOption);
            var maxParallelism = parseResult.GetValue(maxParallelismOption);
            var retryMax = parseResult.GetValue(retryMaxOption);
            var retryBaseDelay = parseResult.GetValue(retryBaseDelayOption);
            var retryMaxDelay = parseResult.GetValue(retryMaxDelayOption);
            var rollbackEnabled = parseResult.GetValue(rollbackOption);
            var snapshotId = parseResult.GetValue(snapshotIdOption);
            var reportJson = parseResult.GetValue(reportJsonOption);

            if (!File.Exists(planPath))
            {
                Console.Error.WriteLine("plan-path: Write plan file not found.");
                return ExitCodes.FatalError;
            }

            var plan = Load<WritePlan>(planPath);
            if (rollbackEnabled && plan.RollbackPlan is null)
            {
                plan.RollbackPlan = new RollbackPlan { Enabled = true, SnapshotId = snapshotId };
            }

            var correlationId = WriteLogging.CreateCorrelationId();
            Console.WriteLine(WriteLogging.BuildStructuredLog(
                correlationId,
                "apply.start",
                new Dictionary<string, string?>
                {
                    ["endpoint"] = endpoint,
                    ["planPath"] = planPath,
                    ["maxParallelism"] = maxParallelism.ToString(),
                    ["retryMax"] = retryMax.ToString(),
                    ["retryBaseDelay"] = retryBaseDelay.ToString(),
                    ["retryMaxDelay"] = retryMaxDelay.ToString(),
                    ["rollback"] = rollbackEnabled.ToString(),
                    ["snapshotId"] = snapshotId
                }));

            var client = new ConfigurationClient(new Uri(endpoint), new DefaultAzureCredential());
            var executor = new WriteExecutor(client);
            var options = new WriteOptions
            {
                MaxParallelism = Math.Max(1, maxParallelism),
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = Math.Max(1, retryMax),
                    BaseDelaySeconds = Math.Max(1, retryBaseDelay),
                    MaxDelaySeconds = Math.Max(1, retryMaxDelay)
                },
                RollbackPlan = rollbackEnabled
                    ? new RollbackPlan { Enabled = true, SnapshotId = snapshotId }
                    : null
            };

            var report = await executor.ExecuteAsync(plan, options, cancellationToken);
            report.CorrelationId = correlationId;

            Console.WriteLine(WriteLogging.BuildStructuredLog(
                correlationId,
                "apply.complete",
                new Dictionary<string, string?>
                {
                    ["creates"] = report.Totals.CreateCount.ToString(),
                    ["updates"] = report.Totals.UpdateCount.ToString(),
                    ["skips"] = report.Totals.SkipCount.ToString(),
                    ["failed"] = report.Totals.FailedCount.ToString()
                }));

            if (!string.IsNullOrWhiteSpace(reportJson))
            {
                var reportWriter = new ReportWriter();
                await reportWriter.WriteWriteReportJsonAsync(report, reportJson, cancellationToken);
            }

            return report.Totals.FailedCount > 0 ? ExitCodes.PartialFailures : ExitCodes.SuccessNoChanges;
        });

        return command;
    }

    private static T Load<T>(string path)
    {
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data is null)
        {
            throw new InvalidOperationException($"Failed to load {typeof(T).Name} from '{path}'.");
        }

        return data;
    }
}
