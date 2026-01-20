using KeyVaultToAppConfig.Core.Mapping;

namespace KeyVaultToAppConfig.Services.Mapping;

public interface IMappingEngine
{
    Task<MappingRun> ExecuteAsync(
        MappingSpecification specification,
        IReadOnlyList<string> sourceKeys,
        CancellationToken cancellationToken);
}
