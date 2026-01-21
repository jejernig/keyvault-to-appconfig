namespace KeyVaultToAppConfig.Core.Planning;

public sealed class DesiredState
{
    public string DesiredStateId { get; set; } = Guid.NewGuid().ToString("N");
    public List<DesiredEntry> Entries { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? GeneratedBy { get; set; }
}
