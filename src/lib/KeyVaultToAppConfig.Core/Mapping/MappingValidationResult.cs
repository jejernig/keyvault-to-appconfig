namespace KeyVaultToAppConfig.Core.Mapping;

public sealed class MappingValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<SpecValidationError> Errors { get; } = new();
}
