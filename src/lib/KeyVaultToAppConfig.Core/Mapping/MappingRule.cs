namespace KeyVaultToAppConfig.Core.Mapping;

public sealed class MappingRule
{
    public string RuleId { get; set; } = string.Empty;
    public MappingStrategyType StrategyType { get; set; } = MappingStrategyType.Direct;
    public string SourceSelector { get; set; } = string.Empty;
    public string TargetKey { get; set; } = string.Empty;
    public int Priority { get; set; }
    public List<TransformOperation> Transforms { get; set; } = new();
}

public enum MappingStrategyType
{
    Direct,
    Regex
}
