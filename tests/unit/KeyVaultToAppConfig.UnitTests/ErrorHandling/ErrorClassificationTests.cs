using KeyVaultToAppConfig.Core.Errors;
using KeyVaultToAppConfig.Services.Errors;

namespace KeyVaultToAppConfig.UnitTests.ErrorHandling;

public sealed class ErrorClassificationTests
{
    [Fact]
    public void Classify_RunScope_ReturnsFatal()
    {
        var classifier = new ErrorClassifier();

        var result = classifier.Classify("run", new InvalidOperationException("boom"));

        Assert.Equal(ErrorClassification.Fatal, result);
    }

    [Fact]
    public void Classify_SecretScope_ReturnsRecoverable()
    {
        var classifier = new ErrorClassifier();

        var result = classifier.Classify("secret", new InvalidOperationException("boom"));

        Assert.Equal(ErrorClassification.Recoverable, result);
    }
}
