using System.Text.RegularExpressions;
using KeyVaultToAppConfig.Core.Mapping;

namespace KeyVaultToAppConfig.Services.Mapping;

public sealed class MappingEngine : IMappingEngine
{
    public Task<MappingRun> ExecuteAsync(
        MappingSpecification specification,
        IReadOnlyList<string> sourceKeys,
        CancellationToken cancellationToken)
    {
        if (specification is null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        if (sourceKeys is null)
        {
            throw new ArgumentNullException(nameof(sourceKeys));
        }

        var run = new MappingRun
        {
            SpecificationId = specification.SpecificationId,
            SpecificationVersion = specification.Version,
            StartedAt = DateTimeOffset.UtcNow,
            Status = MappingRunStatus.Succeeded
        };

        var orderedRules = DeterministicOrder.OrderRules(specification.Rules);
        var output = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var sourceByNormalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var collisionReporter = new CollisionReporter();

        foreach (var sourceKey in DeterministicOrder.PreserveInputOrder(sourceKeys))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var matchResult = FindRule(orderedRules, sourceKey);

            if (matchResult is null)
            {
                if (specification.DefaultBehavior == MappingDefaultBehavior.PassThrough)
                {
                    output[sourceKey] = sourceKey;
                    sourceByNormalized[sourceKey] = sourceKey;
                }
                else
                {
                    run.Status = MappingRunStatus.Failed;
                }

                continue;
            }

            var (rule, match) = matchResult.Value;
            var normalizedKey = ApplyTarget(rule, match);
            normalizedKey = ApplyTransforms(normalizedKey, rule.Transforms, match);

            if (output.ContainsKey(normalizedKey))
            {
                var existingSource = sourceByNormalized[normalizedKey];
                collisionReporter.Register(normalizedKey, existingSource, sourceKey, specification.CollisionPolicy);

                switch (specification.CollisionPolicy)
                {
                    case CollisionPolicy.Error:
                        run.Status = MappingRunStatus.Failed;
                        break;
                    case CollisionPolicy.KeepFirst:
                        break;
                    case CollisionPolicy.KeepLast:
                        output[normalizedKey] = sourceKey;
                        sourceByNormalized[normalizedKey] = sourceKey;
                        break;
                    case CollisionPolicy.ReportOnly:
                        break;
                }

                continue;
            }

            output[normalizedKey] = sourceKey;
            sourceByNormalized[normalizedKey] = sourceKey;
        }

        run.CollisionReport = collisionReporter.BuildReport();
        run.NormalizedKeys = output;
        run.CompletedAt = DateTimeOffset.UtcNow;

        return Task.FromResult(run);
    }

    private static (MappingRule Rule, Match? Match)? FindRule(
        IEnumerable<(MappingRule Rule, int Index)> orderedRules,
        string sourceKey)
    {
        foreach (var (rule, _) in orderedRules)
        {
            if (rule.StrategyType == MappingStrategyType.Direct &&
                string.Equals(rule.SourceSelector, sourceKey, StringComparison.Ordinal))
            {
                return (rule, null);
            }

            if (rule.StrategyType == MappingStrategyType.Regex)
            {
                var match = Regex.Match(sourceKey, rule.SourceSelector, RegexOptions.CultureInvariant);
                if (match.Success)
                {
                    return (rule, match);
                }
            }
        }

        return null;
    }

    private static string ApplyTarget(MappingRule rule, Match? match)
    {
        if (rule.StrategyType == MappingStrategyType.Regex && match is not null)
        {
            return match.Result(rule.TargetKey);
        }

        return rule.TargetKey;
    }

    private static string ApplyTransforms(string input, IEnumerable<TransformOperation> transforms, Match? match)
    {
        var result = input;
        foreach (var transform in transforms)
        {
            result = transform.TransformType switch
            {
                TransformType.Trim => result.Trim(),
                TransformType.Upper => result.ToUpperInvariant(),
                TransformType.Lower => result.ToLowerInvariant(),
                TransformType.Prefix => Prefix(result, transform),
                TransformType.Suffix => Suffix(result, transform),
                TransformType.Replace => ReplaceLiteral(result, transform),
                TransformType.RegexReplace => ReplaceRegex(result, transform),
                TransformType.CaptureSubstitute => CaptureSubstitute(result, transform, match),
                _ => result
            };
        }

        return result;
    }

    private static string Prefix(string value, TransformOperation transform)
    {
        return transform.Parameters.TryGetValue("value", out var prefix)
            ? $"{prefix}{value}"
            : value;
    }

    private static string Suffix(string value, TransformOperation transform)
    {
        return transform.Parameters.TryGetValue("value", out var suffix)
            ? $"{value}{suffix}"
            : value;
    }

    private static string ReplaceLiteral(string value, TransformOperation transform)
    {
        if (!transform.Parameters.TryGetValue("pattern", out var pattern) ||
            !transform.Parameters.TryGetValue("replacement", out var replacement))
        {
            return value;
        }

        return value.Replace(pattern, replacement, StringComparison.Ordinal);
    }

    private static string ReplaceRegex(string value, TransformOperation transform)
    {
        if (!transform.Parameters.TryGetValue("pattern", out var pattern) ||
            !transform.Parameters.TryGetValue("replacement", out var replacement))
        {
            return value;
        }

        return Regex.Replace(value, pattern, replacement, RegexOptions.CultureInvariant);
    }

    private static string CaptureSubstitute(string value, TransformOperation transform, Match? match)
    {
        if (match is null)
        {
            return value;
        }

        if (transform.Parameters.TryGetValue("template", out var template))
        {
            return match.Result(template);
        }

        return value;
    }
}
