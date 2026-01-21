using KeyVaultToAppConfig.Core.Writes;

namespace KeyVaultToAppConfig.Services.Writes;

public interface IWriteExecutor
{
    Task<WriteReport> ExecuteAsync(
        WritePlan plan,
        WriteOptions options,
        CancellationToken cancellationToken);
}
