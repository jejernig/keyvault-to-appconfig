using System.CommandLine;
using KeyVaultToAppConfig.Core;
using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Services.Mapping;

namespace KeyVaultToAppConfig.Cli.Commands;

public static class MappingValidateCommand
{
    public static Command Build()
    {
        var specPathOption = new Option<string>("--spec-path") { Required = true };

        var command = new Command("mapping-validate", "Validate a mapping specification")
        {
            specPathOption
        };

        command.SetAction(parseResult =>
        {
            var specPath = parseResult.GetRequiredValue(specPathOption);

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

            Console.WriteLine("Mapping specification is valid.");
            return ExitCodes.SuccessNoChanges;
        });

        return command;
    }
}
