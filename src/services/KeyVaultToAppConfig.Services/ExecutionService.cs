using KeyVaultToAppConfig.Core;

namespace KeyVaultToAppConfig.Services;

public sealed class ExecutionService
{
    public Task<RunReport> ExecuteAsync(RunConfiguration config, CancellationToken cancellationToken)
    {
        var report = new RunReport
        {
            RunId = Guid.NewGuid().ToString("N"),
            ExecutionMode = config.ExecutionMode,
            Timestamp = DateTimeOffset.UtcNow,
            Totals = new Totals()
        };

        if (config.ExecutionMode == ExecutionMode.Apply)
        {
            // Apply path placeholder; real implementation will populate changes and failures.
            report.Totals.Scanned = 0;
        }

        return Task.FromResult(report);
    }
}
