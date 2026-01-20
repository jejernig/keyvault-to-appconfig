using KeyVaultToAppConfig.Core.Mapping;
using KeyVaultToAppConfig.Services.Mapping;

namespace KeyVaultToAppConfig.UnitTests.Mapping;

public sealed class MappingSpecValidatorTests
{
    [Fact]
    public void Validate_ValidSpec_ReturnsNoErrors()
    {
        var document = CreateDocument("""
        {
          "name": "Valid",
          "version": "v1",
          "rules": [
            {
              "ruleId": "r1",
              "strategyType": "direct",
              "sourceSelector": "Alpha",
              "targetKey": "alpha",
              "priority": 10
            }
          ]
        }
        """, out var path);

        try
        {
            var validator = new MappingSpecValidator();
            var result = validator.Validate(document);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
        finally
        {
            document.Dispose();
            File.Delete(path);
        }
    }

    [Fact]
    public void Validate_InvalidRegex_ReturnsError()
    {
        var document = CreateDocument("""
        {
          "name": "InvalidRegex",
          "version": "v1",
          "rules": [
            {
              "ruleId": "r1",
              "strategyType": "regex",
              "sourceSelector": "[",
              "targetKey": "alpha",
              "priority": 10
            }
          ]
        }
        """, out var path);

        try
        {
            var validator = new MappingSpecValidator();
            var result = validator.Validate(document);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Field == "rules[0].sourceSelector");
        }
        finally
        {
            document.Dispose();
            File.Delete(path);
        }
    }

    [Fact]
    public void Validate_UnknownField_ReturnsError()
    {
        var document = CreateDocument("""
        {
          "name": "UnknownField",
          "version": "v1",
          "unexpected": true,
          "rules": [
            {
              "ruleId": "r1",
              "strategyType": "direct",
              "sourceSelector": "Alpha",
              "targetKey": "alpha",
              "priority": 10
            }
          ]
        }
        """, out var path);

        try
        {
            var validator = new MappingSpecValidator();
            var result = validator.Validate(document);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Field == "spec.unexpected");
        }
        finally
        {
            document.Dispose();
            File.Delete(path);
        }
    }

    [Fact]
    public void Validate_MissingRequiredField_ReturnsError()
    {
        var document = CreateDocument("""
        {
          "version": "v1",
          "rules": []
        }
        """, out var path);

        try
        {
            var validator = new MappingSpecValidator();
            var result = validator.Validate(document);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Field == "name");
        }
        finally
        {
            document.Dispose();
            File.Delete(path);
        }
    }

    private static MappingSpecDocument CreateDocument(string json, out string path)
    {
        path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        var loader = new MappingSpecLoader();
        return loader.Load(path);
    }
}
