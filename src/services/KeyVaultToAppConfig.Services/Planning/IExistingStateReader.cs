using KeyVaultToAppConfig.Core.Planning;

namespace KeyVaultToAppConfig.Services.Planning;

public interface IExistingStateReader
{
    Task<ExistingStateSnapshot> ReadAsync(
        string? keyPrefix,
        IReadOnlyList<string> labels,
        int? pageSize,
        string? continuationToken,
        CancellationToken cancellationToken);
}
