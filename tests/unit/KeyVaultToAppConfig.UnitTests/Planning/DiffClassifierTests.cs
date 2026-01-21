using KeyVaultToAppConfig.Core.Planning;
using KeyVaultToAppConfig.Services.Planning;

namespace KeyVaultToAppConfig.UnitTests.Planning;

public sealed class DiffClassifierTests
{
    [Fact]
    public void Classify_MissingExistingEntry_IsCreate()
    {
        var classifier = new DiffClassifier();
        var desired = new DesiredEntry { Key = "alpha", Label = "prod", Value = "value" };

        var diff = classifier.Classify(desired, null);

        Assert.Equal(DiffClassification.Create, diff.Classification);
        Assert.Equal("Missing in existing state", diff.Reason);
    }

    [Fact]
    public void Classify_MatchingEntry_IsUnchanged()
    {
        var classifier = new DiffClassifier();
        var desired = new DesiredEntry { Key = "alpha", Label = "prod", Value = "value" };
        var existing = new ExistingEntry { Key = "alpha", Label = "prod", Value = "value" };

        var diff = classifier.Classify(desired, existing);

        Assert.Equal(DiffClassification.Unchanged, diff.Classification);
    }

    [Fact]
    public void Classify_DifferentValue_IsUpdate()
    {
        var classifier = new DiffClassifier();
        var desired = new DesiredEntry { Key = "alpha", Label = "prod", Value = "new" };
        var existing = new ExistingEntry { Key = "alpha", Label = "prod", Value = "old" };

        var diff = classifier.Classify(desired, existing);

        Assert.Equal(DiffClassification.Update, diff.Classification);
        Assert.Equal("Value differs", diff.Reason);
    }
}
