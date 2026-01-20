using KeyVaultToAppConfig.Core.Mapping;

namespace KeyVaultToAppConfig.Services.Mapping;

public interface IMappingSpecStore
{
    Task SaveAsync(MappingSpecification specification, CancellationToken cancellationToken);
    Task<MappingSpecification?> LoadAsync(string name, string version, CancellationToken cancellationToken);
}
