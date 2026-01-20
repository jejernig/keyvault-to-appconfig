namespace KeyVaultToAppConfig.Core.Mapping;

public static class DeterministicOrder
{
    public static IEnumerable<(MappingRule Rule, int Index)> OrderRules(IReadOnlyList<MappingRule> rules)
    {
        return rules
            .Select((rule, index) => (rule, index))
            .OrderByDescending(entry => entry.rule.Priority)
            .ThenBy(entry => entry.index);
    }

    public static IEnumerable<string> PreserveInputOrder(IEnumerable<string> sourceKeys)
    {
        return sourceKeys.ToList();
    }
}
