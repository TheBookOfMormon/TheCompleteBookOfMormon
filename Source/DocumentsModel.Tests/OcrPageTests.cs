using System.Collections.Immutable;
using DocumentsModel;


namespace DocumentsModel.Tests;

public class OcrPageTests
{
    private static OcrElement CreateElement(string text = "word", int x = 0, int y = 0, int width = 50, int height = 20, bool isOnNextPage = false)
    {
        return new OcrElement {
            Text = text,
            Bounds = new OcrRect { X = x, Y = y, Width = width, Height = height },
            IsOnNextPage = isOnNextPage
        };
    }

    private static OcrWord CreateWord(string text, int x = 0, int y = 0, int width = 50, int height = 20)
    {
        return new OcrWord {
            Elements = [CreateElement(text, x, y, width, height)]
        };
    }

    private static OcrPage CreatePage(int pageNumber, params string[] wordTexts)
    {
        var words = wordTexts.Select((text, i) =>
            (OcrWord?)CreateWord(text, x: i * 60, y: 0, width: 50, height: 20)
        ).ToImmutableList();

        return new OcrPage {
            PageNumber = pageNumber,
            ImageWidth = 1000,
            ImageHeight = 800,
            Words = words
        };
    }

    // --- AddWord ---

    [Fact]
    public void AddWord_InsertsAtIndex_IncreasesCount()
    {
        var page = CreatePage(1, "alpha", "beta");
        var newWord = CreateWord("gamma");

        var result = OcrPage.AddWord(page, newWord, 1);

        Assert.Equal(3, result.Words.Count);
        Assert.Equal("gamma", result.Words[1]!.GetCombinedText());
    }

    [Fact]
    public void AddWord_AtStart_InsertsAtBeginning()
    {
        var page = CreatePage(1, "beta", "gamma");
        var newWord = CreateWord("alpha");

        var result = OcrPage.AddWord(page, newWord, 0);

        Assert.Equal(3, result.Words.Count);
        Assert.Equal("alpha", result.Words[0]!.GetCombinedText());
        Assert.Equal("beta", result.Words[1]!.GetCombinedText());
    }

    [Fact]
    public void AddWord_AtEnd_AppendsWord()
    {
        var page = CreatePage(1, "alpha", "beta");
        var newWord = CreateWord("gamma");

        var result = OcrPage.AddWord(page, newWord, 2);

        Assert.Equal(3, result.Words.Count);
        Assert.Equal("gamma", result.Words[2]!.GetCombinedText());
    }

    [Fact]
    public void AddWord_OriginalPageUnmodified()
    {
        var page = CreatePage(1, "alpha", "beta");
        var newWord = CreateWord("gamma");

        OcrPage.AddWord(page, newWord, 1);

        Assert.Equal(2, page.Words.Count);
    }

    [Fact]
    public void AddWord_NullWord_InsertsNull()
    {
        var page = CreatePage(1, "alpha");

        var result = OcrPage.AddWord(page, null, 0);

        Assert.Equal(2, result.Words.Count);
        Assert.Null(result.Words[0]);
    }

    // --- DeleteWord ---

    [Fact]
    public void DeleteWord_RemovesAtIndex_DecreasesCount()
    {
        var page = CreatePage(1, "alpha", "beta", "gamma");

        var result = OcrPage.DeleteWord(page, 1);

        Assert.Equal(2, result.Words.Count);
        Assert.Equal("alpha", result.Words[0]!.GetCombinedText());
        Assert.Equal("gamma", result.Words[1]!.GetCombinedText());
    }

    [Fact]
    public void DeleteWord_SetsManuallyEditedTrue()
    {
        var page = CreatePage(1, "alpha", "beta");

        var result = OcrPage.DeleteWord(page, 0);

        Assert.True(result.ManuallyEdited);
    }

    [Fact]
    public void DeleteWord_OriginalPageUnmodified()
    {
        var page = CreatePage(1, "alpha", "beta");

        OcrPage.DeleteWord(page, 0);

        Assert.Equal(2, page.Words.Count);
        Assert.False(page.ManuallyEdited);
    }

    [Fact]
    public void DeleteWord_FirstWord_RemovesCorrectItem()
    {
        var page = CreatePage(1, "alpha", "beta");

        var result = OcrPage.DeleteWord(page, 0);

        Assert.Single(result.Words);
        Assert.Equal("beta", result.Words[0]!.GetCombinedText());
    }

    [Fact]
    public void DeleteWord_LastWord_RemovesCorrectItem()
    {
        var page = CreatePage(1, "alpha", "beta");

        var result = OcrPage.DeleteWord(page, 1);

        Assert.Single(result.Words);
        Assert.Equal("alpha", result.Words[0]!.GetCombinedText());
    }

    // --- ReplaceWord ---

    [Fact]
    public void ReplaceWord_ReplacesAtIndex_CountUnchanged()
    {
        var page = CreatePage(1, "alpha", "beta", "gamma");
        var replacement = CreateWord("delta");

        var result = OcrPage.ReplaceWord(page, 1, replacement);

        Assert.Equal(3, result.Words.Count);
        Assert.Equal("delta", result.Words[1]!.GetCombinedText());
    }

    [Fact]
    public void ReplaceWord_SetsManuallyEditedTrue()
    {
        var page = CreatePage(1, "alpha");
        var replacement = CreateWord("beta");

        var result = OcrPage.ReplaceWord(page, 0, replacement);

        Assert.True(result.ManuallyEdited);
    }

    [Fact]
    public void ReplaceWord_OriginalPageUnmodified()
    {
        var page = CreatePage(1, "alpha", "beta");
        var replacement = CreateWord("gamma");

        OcrPage.ReplaceWord(page, 0, replacement);

        Assert.Equal("alpha", page.Words[0]!.GetCombinedText());
        Assert.False(page.ManuallyEdited);
    }

    [Fact]
    public void ReplaceWord_PreservesOtherWords()
    {
        var page = CreatePage(1, "alpha", "beta", "gamma");
        var replacement = CreateWord("delta");

        var result = OcrPage.ReplaceWord(page, 1, replacement);

        Assert.Equal("alpha", result.Words[0]!.GetCombinedText());
        Assert.Equal("gamma", result.Words[2]!.GetCombinedText());
    }

    [Fact]
    public void ReplaceWord_WithNull_SetsWordToNull()
    {
        var page = CreatePage(1, "alpha", "beta");

        var result = OcrPage.ReplaceWord(page, 0, null);

        Assert.Null(result.Words[0]);
        Assert.Equal(2, result.Words.Count);
    }

    // --- Preserved metadata ---

    [Fact]
    public void AddWord_PreservesPageMetadata()
    {
        var page = CreatePage(5, "word");
        var newWord = CreateWord("extra");

        var result = OcrPage.AddWord(page, newWord, 0);

        Assert.Equal(5, result.PageNumber);
        Assert.Equal(1000, result.ImageWidth);
        Assert.Equal(800, result.ImageHeight);
    }
}
