using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KeyVaultToAppConfig.Core.Mapping;

public sealed class MappingSpecLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters =
        {
            new LenientEnumConverter<MappingStrategyType>(),
            new LenientEnumConverter<TransformType>(),
            new LenientEnumConverter<MappingDefaultBehavior>(),
            new LenientEnumConverter<CollisionPolicy>()
        }
    };

    public MappingSpecDocument Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Spec path is required.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Spec file not found.", path);
        }

        var extension = Path.GetExtension(path).ToLowerInvariant();
        var content = File.ReadAllText(path);
        JsonDocument document;

        if (extension is ".yaml" or ".yml")
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yamlObject = deserializer.Deserialize(new StringReader(content));
            var json = JsonSerializer.Serialize(yamlObject, JsonOptions);
            document = JsonDocument.Parse(json);
        }
        else
        {
            document = JsonDocument.Parse(content);
        }

        var specification = JsonSerializer.Deserialize<MappingSpecification>(
            document.RootElement.GetRawText(),
            JsonOptions) ?? new MappingSpecification();

        if (string.IsNullOrWhiteSpace(specification.SpecificationId))
        {
            specification.SpecificationId = Guid.NewGuid().ToString("N");
        }

        return new MappingSpecDocument(path, document, specification);
    }

    private sealed class LenientEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var normalized = value.Replace("-", string.Empty, StringComparison.Ordinal)
                        .Replace("_", string.Empty, StringComparison.Ordinal)
                        .Replace(" ", string.Empty, StringComparison.Ordinal);
                    if (Enum.TryParse(normalized, true, out T result))
                    {
                        return result;
                    }
                }

                return default;
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var number))
            {
                return (T)Enum.ToObject(typeof(T), number);
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
