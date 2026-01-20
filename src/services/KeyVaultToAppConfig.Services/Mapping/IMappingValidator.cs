using KeyVaultToAppConfig.Core.Mapping;

namespace KeyVaultToAppConfig.Services.Mapping;

public interface IMappingValidator
{
    MappingValidationResult Validate(MappingSpecDocument document);
}
