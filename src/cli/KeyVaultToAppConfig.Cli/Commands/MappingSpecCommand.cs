using System.CommandLine;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Services.Mapping;

namespace KeyVaultToAppConfig.Cli.Commands;

public static class MappingSpecCommand
{
    public static Command Build()
    {
        var specPathOption = new Option<string>("--spec-path") { Required = true };
        var storePathOption = new Option<string?>("--store-path");

        var command = new Command("mapping-spec", "Store a mapping specification")
        {
            specPathOption,
            storePathOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var specPath = parseResult.GetRequiredValue(specPathOption);
            var storePath = parseResult.GetValue(storePathOption);

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

            var rootPath = storePath ?? Path.Combine(Environment.CurrentDirectory, ".mapping-specs");
            var store = new MappingSpecStore(rootPath);
            await store.SaveAsync(document.Specification, cancellationToken);

            Console.WriteLine($"Stored mapping spec '{document.Specification.Name}' version '{document.Specification.Version}'.");
            return ExitCodes.SuccessNoChanges;
        });

        return command;
    }
}
