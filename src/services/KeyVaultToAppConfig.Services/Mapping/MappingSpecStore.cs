using System.Text.Json;
using KeyVaultToAppConfig.Core.Mapping;

namespace KeyVaultToAppConfig.Services.Mapping;

public sealed class MappingSpecStore : IMappingSpecStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _rootPath;

    public MappingSpecStore(string rootPath)
    {
        _rootPath = rootPath;
    }

    public async Task SaveAsync(MappingSpecification specification, CancellationToken cancellationToken)
    {
        if (specification is null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        Directory.CreateDirectory(_rootPath);
        var path = Path.Combine(_rootPath, BuildFileName(specification.Name, specification.Version));
        var json = JsonSerializer.Serialize(specification, JsonOptions);
        await File.WriteAllTextAsync(path, json, cancellationToken);
    }

    public async Task<MappingSpecification?> LoadAsync(string name, string version, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_rootPath, BuildFileName(name, version));
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<MappingSpecification>(json);
    }

    private static string BuildFileName(string name, string version)
    {
        var safeName = Sanitize(name);
        var safeVersion = Sanitize(version);
        return $"{safeName}__{safeVersion}.json";
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return cleaned.Replace(' ', '-').ToLowerInvariant();
    }
}
