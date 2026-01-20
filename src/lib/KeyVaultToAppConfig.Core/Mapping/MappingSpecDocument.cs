using System.Text.Json;

namespace KeyVaultToAppConfig.Core.Mapping;

public sealed class MappingSpecDocument : IDisposable
{
    private readonly JsonDocument _document;

    public MappingSpecDocument(string sourcePath, JsonDocument document, MappingSpecification specification)
    {
        SourcePath = sourcePath;
        _document = document;
        Specification = specification;
    }

    public string SourcePath { get; }
    public JsonElement Root => _document.RootElement;
    public MappingSpecification Specification { get; }

    public void Dispose()
    {
        _document.Dispose();
    }
}
