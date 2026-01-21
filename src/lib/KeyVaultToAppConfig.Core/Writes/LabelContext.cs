namespace KeyVaultToAppConfig.Core.Writes;

public sealed class LabelContext
{
    public string? EnvironmentLabel { get; set; }
    public bool UseEmptyLabelWhenMissing { get; set; } = true;
}
