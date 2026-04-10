using DocumentsModel;
using System.Collections.Immutable;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.AppLayer.Tests.Helpers;

namespace WordsAnalysis.AppLayer.Tests.Features.SyncDocuments;

public class EditionStateTests
{
    private readonly OcrBookInfo _bookInfo = TestDataBuilder.CreateBookInfo(1830, "TestEdition", "TE");

    // --- Constructor: PageNumberToWordIndex and WordIndexToPageNumber ---

    [Fact]
    public void Constructor_SinglePage_BuildsCorrectPageNumberToWordIndex()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello", "world", "test");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);

        Assert.Equal(0, state.PageNumberToWordIndex[1]);
    }

    [Fact]
    public void Constructor_MultiplePages_BuildsCorrectPageNumberToWordIndex()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "a", "b", "c");
        OcrPage page2 = TestDataBuilder.CreatePage(2, "d", "e");
        OcrPage page3 = TestDataBuilder.CreatePage(3, "f");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page1, page2, page3);

        Assert.Equal(0, state.PageNumberToWordIndex[1]);
        Assert.Equal(3, state.PageNumberToWordIndex[2]);
        Assert.Equal(5, state.PageNumberToWordIndex[3]);
    }

    [Fact]
    public void Constructor_MultiplePages_BuildsCorrectWordIndexToPageNumber()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "a", "b", "c");
        OcrPage page2 = TestDataBuilder.CreatePage(2, "d", "e");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page1, page2);

        Assert.Equal(5, state.WordIndexToPageNumber.Count);
        Assert.Equal(1, state.WordIndexToPageNumber[0]); // "a"
        Assert.Equal(1, state.WordIndexToPageNumber[1]); // "b"
        Assert.Equal(1, state.WordIndexToPageNumber[2]); // "c"
        Assert.Equal(2, state.WordIndexToPageNumber[3]); // "d"
        Assert.Equal(2, state.WordIndexToPageNumber[4]); // "e"
    }

    [Fact]
    public void Constructor_EmptyPages_ProducesEmptyMappings()
    {
        EditionState state = new EditionState(_bookInfo, []);

        Assert.Empty(state.PageNumberToWordIndex);
        Assert.Empty(state.WordIndexToPageNumber);
    }

    // --- GetFirstWordIndexForPage / GetPageNumberForWord ---

    [Fact]
    public void GetFirstWordIndexForPage_ReturnsCorrectIndex()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "a", "b");
        OcrPage page2 = TestDataBuilder.CreatePage(2, "c", "d", "e");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page1, page2);

        Assert.Equal(0, state.GetFirstWordIndexForPage(1));
        Assert.Equal(2, state.GetFirstWordIndexForPage(2));
    }

    [Fact]
    public void GetPageNumberForWord_ValidIndex_ReturnsPageNumber()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "a", "b");
        OcrPage page2 = TestDataBuilder.CreatePage(2, "c", "d");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page1, page2);

        Assert.Equal(1, state.GetPageNumberForWord(0));
        Assert.Equal(1, state.GetPageNumberForWord(1));
        Assert.Equal(2, state.GetPageNumberForWord(2));
        Assert.Equal(2, state.GetPageNumberForWord(3));
    }

    [Fact]
    public void GetPageNumberForWord_NegativeIndex_ReturnsNegativeOne()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "a");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);

        Assert.Equal(-1, state.GetPageNumberForWord(-1));
    }

    [Fact]
    public void GetPageNumberForWord_IndexBeyondEnd_ReturnsNegativeOne()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "a", "b");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);

        Assert.Equal(-1, state.GetPageNumberForWord(99));
    }

    // --- GetWordCount ---

    [Fact]
    public void GetWordCount_ReturnsTotal()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "a", "b");
        OcrPage page2 = TestDataBuilder.CreatePage(2, "c");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page1, page2);

        Assert.Equal(3, state.GetWordCount());
    }

    [Fact]
    public void GetWordCount_EmptyState_ReturnsZero()
    {
        EditionState state = new EditionState(_bookInfo, []);

        Assert.Equal(0, state.GetWordCount());
    }

    // --- AddWord / AddWordInternal ---

    [Fact]
    public void AddWord_After_InsertsWordAfterExisting()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello", "world");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var existingRef = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 0);
        OcrWord newWord = TestDataBuilder.CreateWord("new");

        EditionState result = EditionState.AddWord(state, existingRef, newWord, after: true);

        OcrPage resultPage = result.LoadedPages[1].Page;
        Assert.Equal(3, resultPage.Words.Count);
        Assert.Equal("hello", resultPage.Words[0]!.GetCombinedText());
        Assert.Equal("new", resultPage.Words[1]!.GetCombinedText());
        Assert.Equal("world", resultPage.Words[2]!.GetCombinedText());
    }

    [Fact]
    public void AddWord_Before_InsertsWordBeforeExisting()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello", "world");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var existingRef = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 1);
        OcrWord newWord = TestDataBuilder.CreateWord("new");

        EditionState result = EditionState.AddWord(state, existingRef, newWord, after: false);

        OcrPage resultPage = result.LoadedPages[1].Page;
        Assert.Equal(3, resultPage.Words.Count);
        Assert.Equal("hello", resultPage.Words[0]!.GetCombinedText());
        Assert.Equal("new", resultPage.Words[1]!.GetCombinedText());
        Assert.Equal("world", resultPage.Words[2]!.GetCombinedText());
    }

    [Fact]
    public void AddWord_UpdatesWordIndexToPageNumber()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "a", "b");
        OcrPage page2 = TestDataBuilder.CreatePage(2, "c");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page1, page2);
        var existingRef = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 0);
        OcrWord newWord = TestDataBuilder.CreateWord("new");

        EditionState result = EditionState.AddWord(state, existingRef, newWord, after: true);

        Assert.Equal(4, result.GetWordCount());
        Assert.Equal(1, result.GetPageNumberForWord(0)); // "a"
        Assert.Equal(1, result.GetPageNumberForWord(1)); // "new"
        Assert.Equal(1, result.GetPageNumberForWord(2)); // "b"
        Assert.Equal(2, result.GetPageNumberForWord(3)); // "c"
    }

    [Fact]
    public void AddWord_UpdatesPageNumberToWordIndex_ForSubsequentPages()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "a", "b");
        OcrPage page2 = TestDataBuilder.CreatePage(2, "c");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page1, page2);
        var existingRef = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 0);

        EditionState result = EditionState.AddWord(state, existingRef, TestDataBuilder.CreateWord("x"), after: true);

        // page1 originally had 2 words starting at index 0. After adding, page1 has 3 words.
        // page2 originally started at index 2. After adding 1 word to page1, page2 starts at 3.
        Assert.Equal(0, result.PageNumberToWordIndex[1]);
        Assert.Equal(3, result.PageNumberToWordIndex[2]);
    }

    // --- AddSpacers ---

    [Fact]
    public void AddSpacers_AddsNullWords()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var existingRef = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 0);

        EditionState result = EditionState.AddSpacers(state, existingRef, after: false, count: 3);

        OcrPage resultPage = result.LoadedPages[1].Page;
        Assert.Equal(4, resultPage.Words.Count);
        Assert.Null(resultPage.Words[0]);
        Assert.Null(resultPage.Words[1]);
        Assert.Null(resultPage.Words[2]);
        Assert.Equal("hello", resultPage.Words[3]!.GetCombinedText());
    }

    [Fact]
    public void AddSpacers_ZeroCount_ReturnsUnchanged()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var existingRef = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 0);

        EditionState result = EditionState.AddSpacers(state, existingRef, after: true, count: 0);

        Assert.Same(state, result);
    }

    // --- DeleteWords ---

    [Fact]
    public void DeleteWords_RemovesWordFromPage()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello", "world", "test");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var wordToDelete = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 1);

        EditionState result = EditionState.DeleteWords(state, [wordToDelete]);

        OcrPage resultPage = result.LoadedPages[1].Page;
        Assert.Equal(2, resultPage.Words.Count);
        Assert.Equal("hello", resultPage.Words[0]!.GetCombinedText());
        Assert.Equal("test", resultPage.Words[1]!.GetCombinedText());
    }

    [Fact]
    public void DeleteWords_UpdatesWordCount()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "a", "b", "c");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var wordToDelete = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 0);

        EditionState result = EditionState.DeleteWords(state, [wordToDelete]);

        Assert.Equal(2, result.GetWordCount());
    }

    [Fact]
    public void DeleteWords_MultipleWordsFromSamePage_RemovesAll()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "a", "b", "c", "d");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var delete1 = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 0);
        var delete2 = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 2);

        EditionState result = EditionState.DeleteWords(state, [delete1, delete2]);

        OcrPage resultPage = result.LoadedPages[1].Page;
        Assert.Equal(2, resultPage.Words.Count);
        Assert.Equal("b", resultPage.Words[0]!.GetCombinedText());
        Assert.Equal("d", resultPage.Words[1]!.GetCombinedText());
    }

    [Fact]
    public void DeleteWords_UpdatesPageNumberToWordIndex_ForSubsequentPages()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "a", "b", "c");
        OcrPage page2 = TestDataBuilder.CreatePage(2, "d", "e");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page1, page2);
        var wordToDelete = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 1);

        EditionState result = EditionState.DeleteWords(state, [wordToDelete]);

        Assert.Equal(0, result.PageNumberToWordIndex[1]);
        Assert.Equal(2, result.PageNumberToWordIndex[2]); // was 3, now 2 words on page 1
    }

    // --- ReplaceWord ---

    [Fact]
    public void ReplaceWord_ReplacesWordOnPage()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello", "world");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var wordRef = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 0);
        OcrWord replacement = TestDataBuilder.CreateWord("goodbye");

        EditionState result = EditionState.ReplaceWord(state, wordRef, [replacement]);

        OcrPage resultPage = result.LoadedPages[1].Page;
        Assert.Equal(2, resultPage.Words.Count);
        Assert.Equal("goodbye", resultPage.Words[0]!.GetCombinedText());
        Assert.Equal("world", resultPage.Words[1]!.GetCombinedText());
    }

    [Fact]
    public void ReplaceWord_PreservesWordCount()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "hello", "world");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var wordRef = TestDataBuilder.CreateWordReference(_bookInfo, pageNumber: 1, wordIndex: 0);
        OcrWord replacement = TestDataBuilder.CreateWord("goodbye");

        EditionState result = EditionState.ReplaceWord(state, wordRef, [replacement]);

        Assert.Equal(state.GetWordCount(), result.GetWordCount());
    }

    // --- CanMergeWords ---

    [Fact]
    public void CanMergeWords_ValidHyphenatedPattern_ReturnsTrue()
    {
        OcrWord word1 = TestDataBuilder.CreateWord("right");
        OcrWord hyphen = TestDataBuilder.CreateWord("-");
        OcrWord word2 = TestDataBuilder.CreateWord("eous");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word1, hyphen, word2);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);

        var tuple = Tuple.Create(
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 0),
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 1),
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 2)
        );

        Assert.True(state.CanMergeWords(tuple));
    }

    [Fact]
    public void CanMergeWords_MiddleWordNotHyphen_ReturnsFalse()
    {
        OcrWord word1 = TestDataBuilder.CreateWord("hello");
        OcrWord notHyphen = TestDataBuilder.CreateWord("and");
        OcrWord word2 = TestDataBuilder.CreateWord("world");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word1, notHyphen, word2);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);

        var tuple = Tuple.Create(
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 0),
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 1),
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 2)
        );

        Assert.False(state.CanMergeWords(tuple));
    }

    [Fact]
    public void CanMergeWords_CompositeWord_ReturnsFalse()
    {
        OcrWord composite = TestDataBuilder.CreateCompositeWord("right", "eous");
        OcrWord hyphen = TestDataBuilder.CreateWord("-");
        OcrWord word2 = TestDataBuilder.CreateWord("ness");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, composite, hyphen, word2);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);

        var tuple = Tuple.Create(
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 0),
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 1),
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 2)
        );

        Assert.False(state.CanMergeWords(tuple));
    }

    // --- CanMarkWordAsEditorialFormattingChange ---

    [Fact]
    public void CanMarkWordAsEditorialFormattingChange_SimpleWord_ReturnsTrue()
    {
        OcrWord word = TestDataBuilder.CreateWord("Hello");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var wordRef = TestDataBuilder.CreateWordReference(_bookInfo, 1, 0);

        Assert.True(state.CanMarkWordAsEditorialFormattingChange(wordRef));
    }

    [Fact]
    public void CanMarkWordAsEditorialFormattingChange_CompositeWord_ReturnsFalse()
    {
        OcrWord composite = TestDataBuilder.CreateCompositeWord("right", "eous");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, composite);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var wordRef = TestDataBuilder.CreateWordReference(_bookInfo, 1, 0);

        Assert.False(state.CanMarkWordAsEditorialFormattingChange(wordRef));
    }

    [Fact]
    public void CanMarkWordAsEditorialFormattingChange_AlreadyMarked_ReturnsFalse()
    {
        OcrWord word = new OcrWord {
            Elements = [TestDataBuilder.CreateElement("Hello")],
            BenefitOfDoubt = BenefitOfDoubt.EditorialFormatting,
            BenefitOfDoubtText = "hello"
        };
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var wordRef = TestDataBuilder.CreateWordReference(_bookInfo, 1, 0);

        Assert.False(state.CanMarkWordAsEditorialFormattingChange(wordRef));
    }

    // --- MarkWordAsEditorialFormattingChange ---

    [Fact]
    public void MarkWordAsEditorialFormattingChange_LowercasesText()
    {
        OcrWord word = TestDataBuilder.CreateWord("Hello");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var wordRef = TestDataBuilder.CreateWordReference(_bookInfo, 1, 0);

        EditionState result = EditionState.MarkWordAsEditorialFormattingChange(state, wordRef);

        OcrWord? resultWord = wordRef.GetWord(result);
        Assert.NotNull(resultWord);
        Assert.Equal(BenefitOfDoubt.EditorialFormatting, resultWord.BenefitOfDoubt);
        Assert.Equal("hello", resultWord.BenefitOfDoubtText);
    }

    [Fact]
    public void MarkWordAsEditorialFormattingChange_Ampersand_ConvertsToAnd()
    {
        OcrWord word = TestDataBuilder.CreateWord("&");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var wordRef = TestDataBuilder.CreateWordReference(_bookInfo, 1, 0);

        EditionState result = EditionState.MarkWordAsEditorialFormattingChange(state, wordRef);

        OcrWord? resultWord = wordRef.GetWord(result);
        Assert.NotNull(resultWord);
        Assert.Equal("and", resultWord.BenefitOfDoubtText);
    }

    [Fact]
    public void MarkWordAsEditorialFormattingChange_AlreadyLowercase_ReturnsUnchanged()
    {
        OcrWord word = TestDataBuilder.CreateWord("hello");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var wordRef = TestDataBuilder.CreateWordReference(_bookInfo, 1, 0);

        EditionState result = EditionState.MarkWordAsEditorialFormattingChange(state, wordRef);

        // If text is already lowercase, the method returns the original state unchanged
        OcrWord? resultWord = wordRef.GetWord(result);
        Assert.NotNull(resultWord);
        Assert.Equal(BenefitOfDoubt.None, resultWord.BenefitOfDoubt);
    }

    // --- NukeTheRestOfThePage ---

    [Fact]
    public void NukeTheRestOfThePage_RemovesWordsFromSelectedOnward()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "keep1", "keep2", "delete1", "delete2", "delete3");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var selectedRef = TestDataBuilder.CreateWordReference(_bookInfo, 1, 2);

        EditionState result = EditionState.NukeTheRestOfThePage(state, selectedRef);

        OcrPage resultPage = result.LoadedPages[1].Page;
        Assert.Equal(2, resultPage.Words.Count);
        Assert.Equal("keep1", resultPage.Words[0]!.GetCombinedText());
        Assert.Equal("keep2", resultPage.Words[1]!.GetCombinedText());
    }

    [Fact]
    public void NukeTheRestOfThePage_SelectedIsFirstWord_RemovesAllWords()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "a", "b", "c");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);
        var selectedRef = TestDataBuilder.CreateWordReference(_bookInfo, 1, 0);

        EditionState result = EditionState.NukeTheRestOfThePage(state, selectedRef);

        OcrPage resultPage = result.LoadedPages[1].Page;
        Assert.Empty(resultPage.Words);
    }

    [Fact]
    public void NukeTheRestOfThePage_DoesNotAffectOtherPages()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "a", "b", "c");
        OcrPage page2 = TestDataBuilder.CreatePage(2, "d", "e");
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page1, page2);
        var selectedRef = TestDataBuilder.CreateWordReference(_bookInfo, 1, 1);

        EditionState result = EditionState.NukeTheRestOfThePage(state, selectedRef);

        OcrPage resultPage2 = result.LoadedPages[2].Page;
        Assert.Equal(2, resultPage2.Words.Count);
        Assert.Equal("d", resultPage2.Words[0]!.GetCombinedText());
    }

    // --- MergeWords ---

    [Fact]
    public void MergeWords_CombinesThreeWordsIntoComposite()
    {
        OcrWord word1 = TestDataBuilder.CreateWord("right");
        OcrWord hyphen = TestDataBuilder.CreateWord("-");
        OcrWord word2 = TestDataBuilder.CreateWord("eous");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word1, hyphen, word2);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);

        var tuple = Tuple.Create(
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 0),
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 1),
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 2)
        );

        EditionState result = EditionState.MergeWords(state, tuple);

        OcrPage resultPage = result.LoadedPages[1].Page;
        Assert.Single(resultPage.Words);
        OcrWord? merged = resultPage.Words[0];
        Assert.NotNull(merged);
        Assert.True(merged.IsComposite());
        Assert.Equal(3, merged.Elements.Count);
        Assert.Equal("right", merged.Elements[0].Text);
        Assert.Equal("-", merged.Elements[1].Text);
        Assert.Equal("eous", merged.Elements[2].Text);
    }

    [Fact]
    public void MergeWords_InvalidPattern_ReturnsUnchanged()
    {
        OcrWord word1 = TestDataBuilder.CreateWord("hello");
        OcrWord notHyphen = TestDataBuilder.CreateWord("and");
        OcrWord word2 = TestDataBuilder.CreateWord("world");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word1, notHyphen, word2);
        EditionState state = TestDataBuilder.CreateEditionState(_bookInfo, page);

        var tuple = Tuple.Create(
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 0),
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 1),
            TestDataBuilder.CreateWordReference(_bookInfo, 1, 2)
        );

        EditionState result = EditionState.MergeWords(state, tuple);

        // Should return unchanged
        Assert.Equal(3, result.LoadedPages[1].Page.Words.Count);
    }
}
