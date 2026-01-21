using Azure.Data.AppConfiguration;
using KeyVaultToAppConfig.Core.Writes;

namespace KeyVaultToAppConfig.Services.Writes;

public sealed class RollbackHandler
{
    private readonly ConfigurationClient _client;

    public RollbackHandler(ConfigurationClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<bool> TryRollbackAsync(
        WriteResult failedResult,
        WriteAction rollbackAction,
        CancellationToken cancellationToken)
    {
        if (failedResult is null)
        {
            throw new ArgumentNullException(nameof(failedResult));
        }

        if (rollbackAction is null)
        {
            throw new ArgumentNullException(nameof(rollbackAction));
        }

        if (rollbackAction.DesiredValue is null)
        {
            return false;
        }

        var setting = new ConfigurationSetting(rollbackAction.Key, rollbackAction.DesiredValue)
        {
            Label = rollbackAction.Label,
            ContentType = rollbackAction.DesiredContentType
        };

        await _client.SetConfigurationSettingAsync(setting, false, cancellationToken);
        failedResult.Status = WriteStatus.RolledBack;
        failedResult.FailureReason = null;
        return true;
    }
}
