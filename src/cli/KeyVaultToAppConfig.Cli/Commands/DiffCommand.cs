using System.CommandLine;
using System.Text.Json;
using KeyVaultToAppConfig.Cli;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Services.Planning;

namespace KeyVaultToAppConfig.Cli.Commands;

public static class DiffCommand
{
    public static Command Build()
    {
        var desiredPathOption = new Option<string>("--desired-path") { Required = true };
        var existingPathOption = new Option<string>("--existing-path") { Required = true };
        var keyPrefixOption = new Option<string?>("--key-prefix");
        var labelOption = new Option<string[]>("--label") { Arity = ArgumentArity.ZeroOrMore };
        var pageSizeOption = new Option<int?>("--page-size");
        var continuationTokenOption = new Option<string?>("--continuation-token");
        var reportJsonOption = new Option<string?>("--report-json");

        var command = new Command("diff", "Compute diff between desired and existing state")
        {
            desiredPathOption,
            existingPathOption,
            keyPrefixOption,
            labelOption,
            pageSizeOption,
            continuationTokenOption,
            reportJsonOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var desiredPath = parseResult.GetRequiredValue(desiredPathOption);
            var existingPath = parseResult.GetRequiredValue(existingPathOption);
            var keyPrefix = parseResult.GetValue(keyPrefixOption);
            var labels = PlanningCommandHelpers.NormalizeLabels(parseResult.GetValue(labelOption));
            var pageSize = parseResult.GetValue(pageSizeOption);
            var continuationToken = parseResult.GetValue(continuationTokenOption);
            var reportJson = parseResult.GetValue(reportJsonOption);

            if (!File.Exists(desiredPath))
            {
                Console.Error.WriteLine("desired-path: Desired state file not found.");
                return ExitCodes.FatalError;
            }

            if (!File.Exists(existingPath))
            {
                Console.Error.WriteLine("existing-path: Existing state file not found.");
                return ExitCodes.FatalError;
            }

            var correlationId = PlanningLogging.CreateCorrelationId();
            Console.WriteLine(PlanningLogging.BuildStructuredLog(
                correlationId,
                "diff.start",
                new Dictionary<string, string?>
                {
                    ["desiredPath"] = desiredPath,
                    ["existingPath"] = existingPath,
                    ["keyPrefix"] = keyPrefix,
                    ["labels"] = string.Join(",", labels),
                    ["pageSize"] = pageSize?.ToString(),
                    ["continuationToken"] = continuationToken
                }));

            var desiredState = Load<DesiredState>(desiredPath);
            var existingState = Load<ExistingStateSnapshot>(existingPath);

            existingState = PlanningCommandHelpers.ApplyScope(
                existingState,
                keyPrefix,
                labels,
                pageSize,
                continuationToken,
                out var scopeError);
            if (scopeError is not null)
            {
                Console.Error.WriteLine(scopeError);
                return ExitCodes.FatalError;
            }

            var engine = new PlanningEngine();
            var plan = await engine.BuildPlanAsync(desiredState, existingState, cancellationToken);

            foreach (var item in plan.DiffItems)
            {
                Console.WriteLine($"{item.Classification}: {item.Key} [{item.Label}] - {item.Reason}");
            }

            foreach (var conflict in plan.Conflicts)
            {
                Console.WriteLine(
                    $"conflict: {conflict.Key} [{conflict.Label}] values={string.Join(", ", conflict.ConflictingValues)}");
            }

            Console.WriteLine(PlanningLogging.BuildStructuredLog(
                correlationId,
                "diff.complete",
                new Dictionary<string, string?>
                {
                    ["creates"] = plan.Totals.CreateCount.ToString(),
                    ["updates"] = plan.Totals.UpdateCount.ToString(),
                    ["unchanged"] = plan.Totals.UnchangedCount.ToString(),
                    ["conflicts"] = plan.Totals.ConflictCount.ToString()
                }));

            if (!string.IsNullOrWhiteSpace(reportJson))
            {
                var reportWriter = new ReportWriter();
                await reportWriter.WritePlanJsonAsync(plan, correlationId, reportJson, cancellationToken);
            }

            return ExitCodes.SuccessNoChanges;
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
