using System.Drawing;
using ConvertImagesToText;

namespace ConvertImagesToText.Tests;

public class OcrProcessorTests
{
    [Fact]
    public void EstimateWordSize_EmptyString_ReturnsZeroWidth()
    {
        Size result = OcrProcessor.EstimateWordSize(20, "");

        Assert.Equal(0, result.Width);
        Assert.Equal(20, result.Height);
    }

    [Fact]
    public void EstimateWordSize_SingleUppercaseM_ReturnsLineHeightAsWidth()
    {
        // M factor = 1.00, so width = 20 * 1.00 = 20
        Size result = OcrProcessor.EstimateWordSize(20, "M");

        Assert.Equal(20, result.Width);
        Assert.Equal(20, result.Height);
    }

    [Fact]
    public void EstimateWordSize_SingleLowercaseI_ReturnsFlooredWidth()
    {
        // i factor = 0.38, so width = floor(20 * 0.38) = floor(7.6) = 7
        Size result = OcrProcessor.EstimateWordSize(20, "i");

        Assert.Equal(7, result.Width);
        Assert.Equal(20, result.Height);
    }

    [Fact]
    public void EstimateWordSize_WordHi_AccumulatesCharWidths()
    {
        // H=0.83, i=0.38 -> width = floor(20*(0.83+0.38)) = floor(24.2) = 24
        Size result = OcrProcessor.EstimateWordSize(20, "Hi");

        Assert.Equal(24, result.Width);
        Assert.Equal(20, result.Height);
    }

    [Fact]
    public void EstimateWordSize_UnknownCharacter_UsesLowercaseMFactor()
    {
        // '@' is unknown, uses 'm' factor = 1.00, so width = floor(20 * 1.00) = 20
        Size result = OcrProcessor.EstimateWordSize(20, "@");

        Assert.Equal(20, result.Width);
        Assert.Equal(20, result.Height);
    }

    [Fact]
    public void EstimateWordSize_HeightAlwaysEqualsLineHeight()
    {
        Size result = OcrProcessor.EstimateWordSize(42, "abc");

        Assert.Equal(42, result.Height);
    }

    [Fact]
    public void EstimateWordSize_MultipleChars_AccumulateWidth()
    {
        // a=0.67, b=0.71, c=0.67 -> width = floor(10*(0.67+0.71+0.67)) = floor(20.5) = 20
        Size result = OcrProcessor.EstimateWordSize(10, "abc");

        Assert.Equal(20, result.Width);
    }

    [Fact]
    public void EstimateWordSize_LineHeightZero_ReturnsAllZeros()
    {
        Size result = OcrProcessor.EstimateWordSize(0, "Hello");

        Assert.Equal(0, result.Width);
        Assert.Equal(0, result.Height);
    }

    [Fact]
    public void EstimateWordSize_SingleW_WidestCharacter()
    {
        // W factor = 1.17, so width = floor(20 * 1.17) = floor(23.4) = 23
        Size result = OcrProcessor.EstimateWordSize(20, "W");

        Assert.Equal(23, result.Width);
        Assert.Equal(20, result.Height);
    }
}
