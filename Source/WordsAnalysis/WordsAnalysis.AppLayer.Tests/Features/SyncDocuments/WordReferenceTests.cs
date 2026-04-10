using DocumentsModel;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.AppLayer.Tests.Helpers;

namespace WordsAnalysis.AppLayer.Tests.Features.SyncDocuments;

public class WordReferenceTests
{
    private readonly OcrBookInfo _book1830 = TestDataBuilder.CreateBookInfo(1830, "Edition1830", "E1");
    private readonly OcrBookInfo _book1837 = TestDataBuilder.CreateBookInfo(1837, "Edition1837", "E2");

    // --- CompareTo ---

    [Fact]
    public void CompareTo_DifferentBooks_SortsbyYear()
    {
        var earlier = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        var later = TestDataBuilder.CreateWordReference(_book1837, pageNumber: 1, wordIndex: 0);

        Assert.True(earlier.CompareTo(later) < 0);
        Assert.True(later.CompareTo(earlier) > 0);
    }

    [Fact]
    public void CompareTo_SameBookDifferentPages_SortsByPageNumber()
    {
        var page1 = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        var page5 = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 5, wordIndex: 0);

        Assert.True(page1.CompareTo(page5) < 0);
        Assert.True(page5.CompareTo(page1) > 0);
    }

    [Fact]
    public void CompareTo_SamePageDifferentWordIndexes_SortsByWordIndex()
    {
        var word0 = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        var word3 = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 3);

        Assert.True(word0.CompareTo(word3) < 0);
        Assert.True(word3.CompareTo(word0) > 0);
    }

    [Fact]
    public void CompareTo_IdenticalReferences_ReturnsZero()
    {
        var ref1 = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 2, wordIndex: 5);
        var ref2 = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 2, wordIndex: 5);

        Assert.Equal(0, ref1.CompareTo(ref2));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var reference = TestDataBuilder.CreateWordReference(_book1830);

        Assert.True(reference.CompareTo(null) > 0);
    }

    [Fact]
    public void CompareTo_SameYearDifferentCode_SortsByCode()
    {
        var bookA = TestDataBuilder.CreateBookInfo(1830, "AAA", "A1");
        var bookZ = TestDataBuilder.CreateBookInfo(1830, "ZZZ", "Z1");
        var refA = TestDataBuilder.CreateWordReference(bookA, pageNumber: 1, wordIndex: 0);
        var refZ = TestDataBuilder.CreateWordReference(bookZ, pageNumber: 1, wordIndex: 0);

        Assert.True(refA.CompareTo(refZ) < 0);
        Assert.True(refZ.CompareTo(refA) > 0);
    }

    // --- GetGlobalReference ---

    [Fact]
    public void GetGlobalReference_ReturnsCorrectFormat()
    {
        var reference = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 42, wordIndex: 7);

        string result = reference.GetGlobalReference();

        Assert.Equal("1830E1:42:7", result);
    }

    [Fact]
    public void GetGlobalReference_DifferentBookInfo_IncludesYearAndShortCode()
    {
        var book = TestDataBuilder.CreateBookInfo(1920, "Special", "SP");
        var reference = TestDataBuilder.CreateWordReference(book, pageNumber: 100, wordIndex: 0);

        string result = reference.GetGlobalReference();

        Assert.Equal("1920SP:100:0", result);
    }

    // --- GetWord ---

    [Fact]
    public void GetWord_ValidReference_ReturnsCorrectWord()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello", "world", "test");
        EditionState edition = TestDataBuilder.CreateEditionState(_book1830, page);
        var reference = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 1);

        OcrWord? result = reference.GetWord(edition);

        Assert.NotNull(result);
        Assert.Equal("world", result.GetCombinedText());
    }

    [Fact]
    public void GetWord_PageNotLoaded_ReturnsNull()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello");
        EditionState edition = TestDataBuilder.CreateEditionState(_book1830, page);
        var reference = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 99, wordIndex: 0);

        OcrWord? result = reference.GetWord(edition);

        Assert.Null(result);
    }

    [Fact]
    public void GetWord_WordIndexOutOfRange_ReturnsNull()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello", "world");
        EditionState edition = TestDataBuilder.CreateEditionState(_book1830, page);
        var reference = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 10);

        OcrWord? result = reference.GetWord(edition);

        Assert.Null(result);
    }
}
