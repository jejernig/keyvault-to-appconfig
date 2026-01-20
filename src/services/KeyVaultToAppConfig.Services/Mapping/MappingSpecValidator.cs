using System.Text.Json;
using System.Text.RegularExpressions;
using KeyVaultToAppConfig.Core.Mapping;

namespace KeyVaultToAppConfig.Services.Mapping;

public sealed class MappingSpecValidator : IMappingValidator
{
    private static readonly HashSet<string> RootFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "specificationId",
        "name",
        "version",
        "description",
        "defaultBehavior",
        "collisionPolicy",
        "createdAt",
        "createdBy",
        "rules"
    };

    private static readonly HashSet<string> RuleFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "ruleId",
        "strategyType",
        "sourceSelector",
        "targetKey",
        "priority",
        "transforms"
    };

    private static readonly HashSet<string> TransformFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "transformType",
        "parameters"
    };

    private static readonly HashSet<string> DefaultBehaviorValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "reject-unmapped",
        "pass-through"
    };

    private static readonly HashSet<string> CollisionPolicyValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "error",
        "keep-first",
        "keep-last",
        "report-only"
    };

    public MappingValidationResult Validate(MappingSpecDocument document)
    {
        var result = new MappingValidationResult();
        var root = document.Root;

        if (root.ValueKind != JsonValueKind.Object)
        {
            result.Errors.Add(new SpecValidationError { Message = "Specification must be a JSON object." });
            return result;
        }

        ValidateUnknownFields(root, RootFields, result, "spec");
        ValidateRequiredString(root, "name", result, string.Empty);
        ValidateRequiredString(root, "version", result, string.Empty);
        ValidateEnumValue(root, "defaultBehavior", DefaultBehaviorValues, result);
        ValidateEnumValue(root, "collisionPolicy", CollisionPolicyValues, result);

        if (!root.TryGetProperty("rules", out var rulesElement) || rulesElement.ValueKind != JsonValueKind.Array)
        {
            result.Errors.Add(new SpecValidationError { Field = "rules", Message = "Rules must be an array." });
            return result;
        }

        var ruleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ruleIndex = 0;
        foreach (var ruleElement in rulesElement.EnumerateArray())
        {
            var rulePath = $"rules[{ruleIndex}]";
            if (ruleElement.ValueKind != JsonValueKind.Object)
            {
                result.Errors.Add(new SpecValidationError { Field = rulePath, Message = "Rule must be an object." });
                ruleIndex++;
                continue;
            }

            ValidateUnknownFields(ruleElement, RuleFields, result, rulePath);
            var ruleId = GetRequiredString(ruleElement, "ruleId", result, rulePath);
            if (!string.IsNullOrWhiteSpace(ruleId) && !ruleIds.Add(ruleId))
            {
                result.Errors.Add(new SpecValidationError
                {
                    Field = $"{rulePath}.ruleId",
                    Message = "Rule identifiers must be unique within a specification."
                });
            }

            var strategyType = GetRequiredString(ruleElement, "strategyType", result, rulePath);
            var sourceSelector = GetRequiredString(ruleElement, "sourceSelector", result, rulePath);
            GetRequiredString(ruleElement, "targetKey", result, rulePath);
            ValidatePriority(ruleElement, rulePath, result);

            if (!string.IsNullOrWhiteSpace(strategyType) &&
                !strategyType.Equals("direct", StringComparison.OrdinalIgnoreCase) &&
                !strategyType.Equals("regex", StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add(new SpecValidationError
                {
                    Field = $"{rulePath}.strategyType",
                    Message = "Strategy type must be 'direct' or 'regex'."
                });
            }

            if (!string.IsNullOrWhiteSpace(strategyType) &&
                strategyType.Equals("regex", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(sourceSelector))
            {
                ValidateRegex(sourceSelector, $"{rulePath}.sourceSelector", result);
            }

            if (ruleElement.TryGetProperty("transforms", out var transformsElement))
            {
                ValidateTransforms(transformsElement, rulePath, result);
            }

            ruleIndex++;
        }

        return result;
    }

    private static void ValidateUnknownFields(
        JsonElement element,
        HashSet<string> allowedFields,
        MappingValidationResult result,
        string path)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (!allowedFields.Contains(property.Name))
            {
                result.Errors.Add(new SpecValidationError
                {
                    Field = $"{path}.{property.Name}",
                    Message = "Unknown field is not allowed."
                });
            }
        }
    }

    private static void ValidateRequiredString(
        JsonElement element,
        string propertyName,
        MappingValidationResult result,
        string pathPrefix)
    {
        GetRequiredString(element, propertyName, result, pathPrefix);
    }

    private static string GetRequiredString(
        JsonElement element,
        string propertyName,
        MappingValidationResult result,
        string pathPrefix)
    {
        if (!element.TryGetProperty(propertyName, out var valueElement) ||
            valueElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(valueElement.GetString()))
        {
            result.Errors.Add(new SpecValidationError
            {
                Field = $"{pathPrefix}.{propertyName}".Trim('.'),
                Message = "Field is required."
            });
            return string.Empty;
        }

        return valueElement.GetString() ?? string.Empty;
    }

    private static void ValidateEnumValue(
        JsonElement element,
        string propertyName,
        HashSet<string> allowedValues,
        MappingValidationResult result)
    {
        if (!element.TryGetProperty(propertyName, out var valueElement))
        {
            return;
        }

        if (valueElement.ValueKind != JsonValueKind.String ||
            !allowedValues.Contains(valueElement.GetString() ?? string.Empty))
        {
            result.Errors.Add(new SpecValidationError
            {
                Field = propertyName,
                Message = $"Value must be one of: {string.Join(", ", allowedValues)}."
            });
        }
    }

    private static void ValidatePriority(JsonElement element, string path, MappingValidationResult result)
    {
        if (!element.TryGetProperty("priority", out var priorityElement) ||
            priorityElement.ValueKind != JsonValueKind.Number ||
            !priorityElement.TryGetInt32(out var priority))
        {
            result.Errors.Add(new SpecValidationError
            {
                Field = $"{path}.priority",
                Message = "Priority must be an integer."
            });
            return;
        }

        if (priority < 0 || priority > 1000)
        {
            result.Errors.Add(new SpecValidationError
            {
                Field = $"{path}.priority",
                Message = "Priority must be between 0 and 1000."
            });
        }
    }

    private static void ValidateTransforms(JsonElement transformsElement, string rulePath, MappingValidationResult result)
    {
        if (transformsElement.ValueKind != JsonValueKind.Array)
        {
            result.Errors.Add(new SpecValidationError
            {
                Field = $"{rulePath}.transforms",
                Message = "Transforms must be an array."
            });
            return;
        }

        var index = 0;
        foreach (var transformElement in transformsElement.EnumerateArray())
        {
            var transformPath = $"{rulePath}.transforms[{index}]";
            if (transformElement.ValueKind != JsonValueKind.Object)
            {
                result.Errors.Add(new SpecValidationError
                {
                    Field = transformPath,
                    Message = "Transform must be an object."
                });
                index++;
                continue;
            }

            ValidateUnknownFields(transformElement, TransformFields, result, transformPath);
            var typeName = GetRequiredString(transformElement, "transformType", result, transformPath);
            if (string.IsNullOrWhiteSpace(typeName))
            {
                index++;
                continue;
            }

            if (!TransformCatalog.TryParse(typeName, out var transformType))
            {
                result.Errors.Add(new SpecValidationError
                {
                    Field = $"{transformPath}.transformType",
                    Message = $"Unsupported transform '{typeName}'."
                });
                index++;
                continue;
            }

            ValidateTransformParameters(transformElement, transformPath, transformType, result);
            index++;
        }
    }

    private static void ValidateTransformParameters(
        JsonElement transformElement,
        string transformPath,
        TransformType transformType,
        MappingValidationResult result)
    {
        if (!transformElement.TryGetProperty("parameters", out var parametersElement))
        {
            parametersElement = default;
        }

        switch (transformType)
        {
            case TransformType.Prefix:
            case TransformType.Suffix:
                RequireParameter(parametersElement, transformPath, "value", result);
                break;
            case TransformType.Replace:
                RequireParameter(parametersElement, transformPath, "pattern", result);
                RequireParameter(parametersElement, transformPath, "replacement", result);
                break;
            case TransformType.RegexReplace:
                RequireParameter(parametersElement, transformPath, "pattern", result);
                RequireParameter(parametersElement, transformPath, "replacement", result);
                if (parametersElement.ValueKind == JsonValueKind.Object &&
                    parametersElement.TryGetProperty("pattern", out var patternElement) &&
                    patternElement.ValueKind == JsonValueKind.String)
                {
                    ValidateRegex(patternElement.GetString() ?? string.Empty, $"{transformPath}.parameters.pattern", result);
                }
                break;
            case TransformType.CaptureSubstitute:
                RequireParameter(parametersElement, transformPath, "template", result);
                break;
        }
    }

    private static void RequireParameter(
        JsonElement parametersElement,
        string transformPath,
        string parameterName,
        MappingValidationResult result)
    {
        if (parametersElement.ValueKind != JsonValueKind.Object ||
            !parametersElement.TryGetProperty(parameterName, out var valueElement) ||
            valueElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(valueElement.GetString()))
        {
            result.Errors.Add(new SpecValidationError
            {
                Field = $"{transformPath}.parameters.{parameterName}",
                Message = "Parameter is required."
            });
        }
    }

    private static void ValidateRegex(string pattern, string field, MappingValidationResult result)
    {
        try
        {
            _ = new Regex(pattern, RegexOptions.CultureInvariant);
        }
        catch (ArgumentException)
        {
            result.Errors.Add(new SpecValidationError
            {
                Field = field,
                Message = "Regex pattern is invalid."
            });
        }
    }
}
