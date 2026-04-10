using DocumentsModel;

namespace DocumentsModel.Tests;

public class BenefitOfDoubtTests
{
    [Fact]
    public void GetOptions_CoversAllEnumValues()
    {
        var options = BenefitOfDoubtExtensions.GetOptions().ToList();
        var allEnumValues = Enum.GetValues<BenefitOfDoubt>();

        foreach (var enumValue in allEnumValues)
        {
            Assert.Contains(options, o => o.Key == enumValue);
        }
    }

    [Fact]
    public void GetOptions_CountMatchesEnumValueCount()
    {
        var options = BenefitOfDoubtExtensions.GetOptions().ToList();
        var allEnumValues = Enum.GetValues<BenefitOfDoubt>();

        Assert.Equal(allEnumValues.Length, options.Count);
    }

    [Theory]
    [InlineData(BenefitOfDoubt.None, "None")]
    [InlineData(BenefitOfDoubt.PrinterError, "Printer error")]
    [InlineData(BenefitOfDoubt.InkError, "Ink error")]
    [InlineData(BenefitOfDoubt.EditorialFormatting, "Editorial formatting")]
    [InlineData(BenefitOfDoubt.MediaOrScanningError, "Media/Scanning error")]
    public void GetOptions_HasExpectedDisplayName(BenefitOfDoubt enumValue, string expectedName)
    {
        var options = BenefitOfDoubtExtensions.GetOptions().ToList();
        var option = options.First(o => o.Key == enumValue);

        Assert.Equal(expectedName, option.Value);
    }

    [Fact]
    public void GetOptions_AllNamesAreNonEmpty()
    {
        var options = BenefitOfDoubtExtensions.GetOptions();

        foreach (var option in options)
        {
            Assert.False(string.IsNullOrWhiteSpace(option.Value),
                $"Display name for {option.Key} should not be empty");
        }
    }

    [Fact]
    public void GetOptions_NoDuplicateKeys()
    {
        var options = BenefitOfDoubtExtensions.GetOptions().ToList();
        var distinctKeys = options.Select(o => o.Key).Distinct().Count();

        Assert.Equal(options.Count, distinctKeys);
    }

    [Fact]
    public void GetOptions_ReturnsKeyValuePairs()
    {
        var options = BenefitOfDoubtExtensions.GetOptions();

        Assert.All(options, option =>
        {
            Assert.IsType<BenefitOfDoubt>(option.Key);
            Assert.IsType<string>(option.Value);
        });
    }

    [Fact]
    public void GetOptions_NoneValueIsZero()
    {
        Assert.Equal(0, (int)BenefitOfDoubt.None);
    }

    [Fact]
    public void GetOptions_AllDisplayNamesAreHumanReadable()
    {
        var options = BenefitOfDoubtExtensions.GetOptions();

        foreach (var option in options)
        {
            // Each name should contain only letters, spaces, and slashes (for "Media/Scanning error")
            Assert.Matches(@"^[A-Za-z/\s]+$", option.Value);
        }
    }
}
