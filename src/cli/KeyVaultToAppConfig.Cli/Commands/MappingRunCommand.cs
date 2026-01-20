using System.CommandLine;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Services.Mapping;

namespace KeyVaultToAppConfig.Cli.Commands;

public static class MappingRunCommand
{
    public static Command Build()
    {
        var specPathOption = new Option<string>("--spec-path") { Required = true };
        var sourcePathOption = new Option<string>("--source-path") { Required = true };

        var command = new Command("mapping-run", "Execute a mapping run for a source key list")
        {
            specPathOption,
            sourcePathOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var specPath = parseResult.GetRequiredValue(specPathOption);
            var sourcePath = parseResult.GetRequiredValue(sourcePathOption);

            if (!File.Exists(sourcePath))
            {
                Console.Error.WriteLine("source-path: Source file not found.");
                return ExitCodes.FatalError;
            }

            var loader = new MappingSpecLoader();
            using var document = loader.Load(specPath);
            var validator = new MappingSpecValidator();
            var validation = validator.Validate(document);
            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    var field = string.IsNullOrWhiteSpace(error.Field) ? "spec" : error.Field;
                    Console.Error.WriteLine($"{field}: {error.Message}");
                }

                return ExitCodes.FatalError;
            }

            var sourceKeys = File.ReadAllLines(sourcePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            var engine = new MappingEngine();
            var run = await engine.ExecuteAsync(document.Specification, sourceKeys, cancellationToken);

            foreach (var entry in run.NormalizedKeys)
            {
                Console.WriteLine($"{entry.Key} <= {entry.Value}");
            }

            if (run.CollisionReport is not null)
            {
                foreach (var collision in run.CollisionReport.Entries)
                {
                    Console.WriteLine(
                        $"collision: {collision.NormalizedKey} [{string.Join(", ", collision.SourceKeys)}] policy={collision.AppliedPolicy}");
                }
            }

            return run.Status == MappingRunStatus.Failed
                ? ExitCodes.PartialFailures
                : ExitCodes.SuccessNoChanges;
        });

        return command;
    }
}
