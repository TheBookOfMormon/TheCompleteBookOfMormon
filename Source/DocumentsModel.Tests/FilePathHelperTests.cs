using DocumentsModel;
using DocumentsModel.Helpers;


namespace DocumentsModel.Tests;

public class FilePathHelperTests
{
    private const string SourcesDir = @"C:\Sources";
    private readonly OcrBookInfo _bookInfo = new OcrBookInfo {
        Year = 1830,
        Code = "1830-Edition",
        Name = "Test Edition 1830",
        ShortCode = "TE"
    };

    // --- GetBookInfoFilePath ---

    [Fact]
    public void GetBookInfoFilePath_CombinesPathWithIndexJson()
    {
        var result = FilePathHelper.GetBookInfoFilePath(SourcesDir, "1830-Edition");

        Assert.Equal(Path.Combine(SourcesDir, "1830-Edition", "index.json"), result);
    }

    [Fact]
    public void GetBookInfoFilePath_DifferentEdition_UsesEditionCode()
    {
        var result = FilePathHelper.GetBookInfoFilePath(SourcesDir, "1837-Edition");

        Assert.Equal(Path.Combine(SourcesDir, "1837-Edition", "index.json"), result);
    }

    // --- GetEditionDirectoryPath (OcrBookInfo overload) ---

    [Fact]
    public void GetEditionDirectoryPath_WithBookInfo_UsesBookInfoCode()
    {
        var result = FilePathHelper.GetEditionDirectoryPath(SourcesDir, _bookInfo);

        Assert.Equal(Path.Combine(SourcesDir, "1830-Edition"), result);
    }

    // --- GetEditionDirectoryPath (string overload) ---

    [Fact]
    public void GetEditionDirectoryPath_WithString_CombinesSourcesDirAndCode()
    {
        var result = FilePathHelper.GetEditionDirectoryPath(SourcesDir, "MyEdition");

        Assert.Equal(Path.Combine(SourcesDir, "MyEdition"), result);
    }

    // --- GetOcrDirectoryPath ---

    [Fact]
    public void GetOcrDirectoryPath_Adds03OcrSubfolder()
    {
        var result = FilePathHelper.GetOcrDirectoryPath(SourcesDir, _bookInfo);

        Assert.Equal(Path.Combine(SourcesDir, "1830-Edition", "03-OCR"), result);
    }

    // --- GetPageFilePath ---

    [Theory]
    [InlineData(1, "001.PageJson")]
    [InlineData(10, "010.PageJson")]
    [InlineData(123, "123.PageJson")]
    public void GetPageFilePath_FormatsPageNumberAsThreeDigits(int pageNumber, string expectedFileName)
    {
        var result = FilePathHelper.GetPageFilePath(SourcesDir, _bookInfo, pageNumber);

        Assert.Equal(Path.Combine(SourcesDir, "1830-Edition", "03-OCR", expectedFileName), result);
    }

    // --- GetPageMetaFilePath ---

    [Theory]
    [InlineData(1, "001.PageMetaJson")]
    [InlineData(42, "042.PageMetaJson")]
    [InlineData(200, "200.PageMetaJson")]
    public void GetPageMetaFilePath_FormatsPageNumberAsThreeDigits(int pageNumber, string expectedFileName)
    {
        var result = FilePathHelper.GetPageMetaFilePath(SourcesDir, _bookInfo, pageNumber);

        Assert.Equal(Path.Combine(SourcesDir, "1830-Edition", "03-OCR", expectedFileName), result);
    }

    // --- GetSamplesDirectoryPath ---

    [Fact]
    public void GetSamplesDirectoryPath_AddsSamplesSubfolder()
    {
        var result = FilePathHelper.GetSamplesDirectoryPath(SourcesDir, _bookInfo);

        Assert.Equal(Path.Combine(SourcesDir, "1830-Edition", "Samples"), result);
    }

    // --- GetScansDeskewedImageFilePath ---

    [Theory]
    [InlineData(1, "001.tif")]
    [InlineData(55, "055.tif")]
    [InlineData(300, "300.tif")]
    public void GetScansDeskewedImageFilePath_FormatsPageNumberWithTifExtension(int pageNumber, string expectedFileName)
    {
        var result = FilePathHelper.GetScansDeskewedImageFilePath(SourcesDir, _bookInfo, pageNumber);

        Assert.Equal(Path.Combine(SourcesDir, "1830-Edition", "02-ScansDeskewed", expectedFileName), result);
    }

    // --- Both overloads of GetEditionDirectoryPath produce same result ---

    [Fact]
    public void GetEditionDirectoryPath_BothOverloads_ProduceSameResult()
    {
        var fromBookInfo = FilePathHelper.GetEditionDirectoryPath(SourcesDir, _bookInfo);
        var fromString = FilePathHelper.GetEditionDirectoryPath(SourcesDir, _bookInfo.Code);

        Assert.Equal(fromBookInfo, fromString);
    }
}
