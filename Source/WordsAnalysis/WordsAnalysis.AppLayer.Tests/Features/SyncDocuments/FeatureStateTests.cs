using DocumentsModel;
using System.Collections.Immutable;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.AppLayer.Tests.Helpers;

namespace WordsAnalysis.AppLayer.Tests.Features.SyncDocuments;

public class FeatureStateTests
{
    private readonly OcrBookInfo _book1 = TestDataBuilder.CreateBookInfo(1830, "Ed1830", "E1");
    private readonly OcrBookInfo _book2 = TestDataBuilder.CreateBookInfo(1837, "Ed1837", "E2");
    private readonly OcrBookInfo _book3 = TestDataBuilder.CreateBookInfo(1840, "Ed1840", "E3");

    private FeatureState CreateTwoEditionState(string[] words1, string[] words2)
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, words1);
        OcrPage page2 = TestDataBuilder.CreatePage(1, words2);
        return TestDataBuilder.CreateFeatureState(
            (_book1, new[] { page1 }),
            (_book2, new[] { page2 })
        );
    }

    private FeatureState WithRowData(FeatureState state, params (OcrBookInfo book, int wordCount)[] rows)
    {
        var rowDataList = new List<RowData>();
        foreach (var (book, wordCount) in rows)
        {
            var wordRefs = Enumerable.Range(0, wordCount)
                .Select(i => new WordReference(book, 1, i))
                .ToImmutableList();
            rowDataList.Add(new RowData { BookInfo = book, Words = wordRefs });
        }
        return state with { RowData = rowDataList.ToImmutableArray() };
    }

    // --- ToggleWordSelected ---

    [Fact]
    public void ToggleWordSelected_SelectsUnselectedWord()
    {
        FeatureState state = CreateTwoEditionState(["hello"], ["hello"]);
        var wordRef = TestDataBuilder.CreateWordReference(_book1, 1, 0);

        FeatureState result = FeatureState.ToggleWordSelected(state, wordRef);

        Assert.True(result.IsWordSelected(wordRef));
    }

    [Fact]
    public void ToggleWordSelected_DeselectsSelectedWord()
    {
        FeatureState state = CreateTwoEditionState(["hello"], ["hello"]);
        var wordRef = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        state = FeatureState.ToggleWordSelected(state, wordRef);

        FeatureState result = FeatureState.ToggleWordSelected(state, wordRef);

        Assert.False(result.IsWordSelected(wordRef));
    }

    // --- IsWordSelected ---

    [Fact]
    public void IsWordSelected_NotSelected_ReturnsFalse()
    {
        FeatureState state = CreateTwoEditionState(["hello"], ["hello"]);
        var wordRef = TestDataBuilder.CreateWordReference(_book1, 1, 0);

        Assert.False(state.IsWordSelected(wordRef));
    }

    // --- DeselectAll ---

    [Fact]
    public void DeselectAll_ClearsAllSelections()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["hello", "world"]);
        var ref1 = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        var ref2 = TestDataBuilder.CreateWordReference(_book2, 1, 0);
        state = FeatureState.ToggleWordSelected(state, ref1);
        state = FeatureState.ToggleWordSelected(state, ref2);

        FeatureState result = FeatureState.DeselectAll(state);

        Assert.Empty(result.SelectedWords);
    }

    // --- SelectWord ---

    [Fact]
    public void SelectWord_SelectsWord()
    {
        FeatureState state = CreateTwoEditionState(["hello"], ["hello"]);
        var wordRef = TestDataBuilder.CreateWordReference(_book1, 1, 0);

        FeatureState result = FeatureState.SelectWord(state, wordRef);

        Assert.True(result.IsWordSelected(wordRef));
    }

    [Fact]
    public void SelectWord_AlreadySelected_DoesNotDuplicate()
    {
        FeatureState state = CreateTwoEditionState(["hello"], ["hello"]);
        var wordRef = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        state = FeatureState.SelectWord(state, wordRef);

        FeatureState result = FeatureState.SelectWord(state, wordRef);

        Assert.Single(result.SelectedWords);
    }

    // --- SelectWords ---

    [Fact]
    public void SelectWords_SelectsMultipleWords()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["hello", "world"]);
        var ref1 = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        var ref2 = TestDataBuilder.CreateWordReference(_book2, 1, 0);

        FeatureState result = FeatureState.SelectWords(state, [ref1, ref2]);

        Assert.True(result.IsWordSelected(ref1));
        Assert.True(result.IsWordSelected(ref2));
        Assert.Equal(2, result.SelectedWords.Count);
    }

    // --- CanAlignSelectedWords ---

    [Fact]
    public void CanAlignSelectedWords_TwoSelectionsFromDifferentEditions_ReturnsTrue()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["hello", "world"]);
        var ref1 = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        var ref2 = TestDataBuilder.CreateWordReference(_book2, 1, 0);
        state = FeatureState.SelectWords(state, [ref1, ref2]);

        Assert.True(state.CanAlignSelectedWords());
    }

    [Fact]
    public void CanAlignSelectedWords_OnlyOneSelection_ReturnsFalse()
    {
        FeatureState state = CreateTwoEditionState(["hello"], ["hello"]);
        var ref1 = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        state = FeatureState.SelectWord(state, ref1);

        Assert.False(state.CanAlignSelectedWords());
    }

    [Fact]
    public void CanAlignSelectedWords_TwoSelectionsFromSameEdition_ReturnsFalse()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["hello", "world"]);
        var ref1 = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        var ref2 = TestDataBuilder.CreateWordReference(_book1, 1, 1);
        state = FeatureState.SelectWords(state, [ref1, ref2]);

        Assert.False(state.CanAlignSelectedWords());
    }

    [Fact]
    public void CanAlignSelectedWords_NoSelections_ReturnsFalse()
    {
        FeatureState state = CreateTwoEditionState(["hello"], ["hello"]);

        Assert.False(state.CanAlignSelectedWords());
    }

    // --- AlignSelectedWords ---

    [Fact]
    public void AlignSelectedWords_AddsSpacersToAlignColumns()
    {
        // book1 has word at column 0, book2 has word at column 2
        // After alignment, book1 should get 2 spacers to shift its word to column 2
        OcrPage page1 = TestDataBuilder.CreatePage(1, "target", "b", "c");
        OcrPage page2 = TestDataBuilder.CreatePage(1, "x", "y", "target");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1, new[] { page1 }),
            (_book2, new[] { page2 })
        );

        var ref1 = new WordReference(_book1, 1, 0); // column 0
        var ref2 = new WordReference(_book2, 1, 2); // column 2

        var wordRefs1 = ImmutableList.Create(ref1, new WordReference(_book1, 1, 1), new WordReference(_book1, 1, 2));
        var wordRefs2 = ImmutableList.Create(new WordReference(_book2, 1, 0), new WordReference(_book2, 1, 1), ref2);

        state = state with {
            RowData = ImmutableArray.Create(
                new RowData { BookInfo = _book1, Words = wordRefs1 },
                new RowData { BookInfo = _book2, Words = wordRefs2 }
            )
        };
        state = FeatureState.SelectWords(state, [ref1, ref2]);

        FeatureState result = FeatureState.AlignSelectedWords(state);

        // book1 should have had 2 spacers added before its "target" word
        EditionState edition1 = result.Editions[_book1];
        OcrPage resultPage1 = edition1.LoadedPages[1].Page;
        Assert.Equal(5, resultPage1.Words.Count); // 3 original + 2 spacers
        Assert.Empty(result.SelectedWords); // Selection cleared after alignment
    }

    // --- DeleteSelectedWords ---

    [Fact]
    public void DeleteSelectedWords_RemovesSelectedWords()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["hello", "world"]);
        var wordRef = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        state = FeatureState.SelectWord(state, wordRef);

        FeatureState result = FeatureState.DeleteSelectedWords(state);

        EditionState edition1 = result.Editions[_book1];
        Assert.Single(edition1.LoadedPages[1].Page.Words);
        Assert.Equal("world", edition1.LoadedPages[1].Page.Words[0]!.GetCombinedText());
        Assert.Empty(result.SelectedWords);
    }

    // --- DeleteWords ---

    [Fact]
    public void DeleteWords_EmptyList_ReturnsUnchanged()
    {
        FeatureState state = CreateTwoEditionState(["hello"], ["hello"]);

        FeatureState result = FeatureState.DeleteWords(state, []);

        Assert.Equal(state.Editions[_book1].LoadedPages[1].Page.Words.Count, result.Editions[_book1].LoadedPages[1].Page.Words.Count);
    }

    [Fact]
    public void DeleteWords_FromMultipleEditions_RemovesFromEach()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["foo", "bar"]);
        var refEdition1 = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        var refEdition2 = TestDataBuilder.CreateWordReference(_book2, 1, 0);

        FeatureState result = FeatureState.DeleteWords(state, [refEdition1, refEdition2]);

        Assert.Single(result.Editions[_book1].LoadedPages[1].Page.Words);
        Assert.Equal("world", result.Editions[_book1].LoadedPages[1].Page.Words[0]!.GetCombinedText());
        Assert.Single(result.Editions[_book2].LoadedPages[1].Page.Words);
        Assert.Equal("bar", result.Editions[_book2].LoadedPages[1].Page.Words[0]!.GetCombinedText());
        Assert.Empty(result.SelectedWords);
    }

    // --- MergeWords ---

    [Fact]
    public void MergeWords_MergesHyphenatedWordsInSelectedEdition()
    {
        OcrWord word1 = TestDataBuilder.CreateWord("right");
        OcrWord hyphen = TestDataBuilder.CreateWord("-");
        OcrWord word2 = TestDataBuilder.CreateWord("eous");
        OcrPage page1 = TestDataBuilder.CreatePageWithWords(1, word1, hyphen, word2);
        OcrPage page2 = TestDataBuilder.CreatePage(1, "righteous");

        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1, new[] { page1 }),
            (_book2, new[] { page2 })
        );

        var wordRef0 = new WordReference(_book1, 1, 0);
        var wordRef1 = new WordReference(_book1, 1, 1);
        var wordRef2 = new WordReference(_book1, 1, 2);
        var wordRef2Edition2 = new WordReference(_book2, 1, 0);

        state = state with {
            RowData = ImmutableArray.Create(
                new RowData {
                    BookInfo = _book1,
                    Words = ImmutableList.Create(wordRef0, wordRef1, wordRef2)
                },
                new RowData {
                    BookInfo = _book2,
                    Words = ImmutableList.Create(wordRef2Edition2)
                }
            )
        };
        state = FeatureState.SelectWord(state, wordRef0);

        FeatureState result = FeatureState.MergeWords(state);

        EditionState edition1 = result.Editions[_book1];
        OcrPage resultPage = edition1.LoadedPages[1].Page;
        Assert.Single(resultPage.Words);
        Assert.True(resultPage.Words[0]!.IsComposite());
        Assert.Empty(result.SelectedWords);
    }

    [Fact]
    public void MergeWords_NoMergeableWords_ReturnsUnchanged()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["hello", "world"]);
        var wordRef = TestDataBuilder.CreateWordReference(_book1, 1, 0);

        state = state with {
            RowData = ImmutableArray.Create(
                new RowData {
                    BookInfo = _book1,
                    Words = ImmutableList.Create(
                        new WordReference(_book1, 1, 0),
                        new WordReference(_book1, 1, 1))
                },
                new RowData {
                    BookInfo = _book2,
                    Words = ImmutableList.Create(
                        new WordReference(_book2, 1, 0),
                        new WordReference(_book2, 1, 1))
                }
            )
        };
        state = FeatureState.SelectWord(state, wordRef);

        FeatureState result = FeatureState.MergeWords(state);

        // No merge happened - words still exist separately
        Assert.Equal(2, result.Editions[_book1].LoadedPages[1].Page.Words.Count);
    }

    // --- GetWordGridLocation ---

    [Fact]
    public void GetWordGridLocation_ReturnsCorrectPosition()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["foo", "bar"]);
        var ref1_0 = new WordReference(_book1, 1, 0);
        var ref1_1 = new WordReference(_book1, 1, 1);
        var ref2_0 = new WordReference(_book2, 1, 0);
        var ref2_1 = new WordReference(_book2, 1, 1);

        state = state with {
            RowData = ImmutableArray.Create(
                new RowData { BookInfo = _book1, Words = ImmutableList.Create(ref1_0, ref1_1) },
                new RowData { BookInfo = _book2, Words = ImmutableList.Create(ref2_0, ref2_1) }
            )
        };

        var (col, row) = state.GetWordGridLocation(ref2_1);

        Assert.Equal(1, col); // second column
        Assert.Equal(1, row); // second row
    }

    [Fact]
    public void GetWordGridLocation_FirstWordFirstRow()
    {
        FeatureState state = CreateTwoEditionState(["hello"], ["world"]);
        var ref1_0 = new WordReference(_book1, 1, 0);
        var ref2_0 = new WordReference(_book2, 1, 0);

        state = state with {
            RowData = ImmutableArray.Create(
                new RowData { BookInfo = _book1, Words = ImmutableList.Create(ref1_0) },
                new RowData { BookInfo = _book2, Words = ImmutableList.Create(ref2_0) }
            )
        };

        var (col, row) = state.GetWordGridLocation(ref1_0);

        Assert.Equal(0, col);
        Assert.Equal(0, row);
    }

    // --- SelectWordRangeInColumn ---

    [Fact]
    public void SelectWordRangeInColumn_SelectsWordsAcrossEditions()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "hello", "world");
        OcrPage page2 = TestDataBuilder.CreatePage(1, "hello", "earth");
        OcrPage page3 = TestDataBuilder.CreatePage(1, "hello", "mars");

        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1, new[] { page1 }),
            (_book2, new[] { page2 }),
            (_book3, new[] { page3 })
        );

        var ref1 = new WordReference(_book1, 1, 0);
        var ref2 = new WordReference(_book2, 1, 0);
        var ref3 = new WordReference(_book3, 1, 0);

        // RowData ordered descending by code (matches GetWordsAsync behaviour)
        // so book3 (1840) first, book2 (1837) second, book1 (1830) last.
        // SelectWordRangeInColumn swaps so firstEdition.CompareTo(lastEdition) >= 0,
        // meaning firstEdition is the higher-year book which appears first in RowData.
        state = state with {
            RowData = ImmutableArray.Create(
                new RowData { BookInfo = _book3, Words = ImmutableList.Create(ref3, new WordReference(_book3, 1, 1)) },
                new RowData { BookInfo = _book2, Words = ImmutableList.Create(ref2, new WordReference(_book2, 1, 1)) },
                new RowData { BookInfo = _book1, Words = ImmutableList.Create(ref1, new WordReference(_book1, 1, 1)) }
            )
        };

        // Select column 0 from book1 to book3. The method swaps so it iterates
        // from book3 (first in RowData) to book1 (last in RowData), selecting all.
        FeatureState result = FeatureState.SelectWordRangeInColumn(state, columnIndex: 0, firstEdition: _book1, lastEdition: _book3);

        Assert.True(result.IsWordSelected(ref1));
        Assert.True(result.IsWordSelected(ref2));
        Assert.True(result.IsWordSelected(ref3));
    }

    // --- SelectWordRangeInEdition ---

    [Fact]
    public void SelectWordRangeInEdition_SelectsContiguousRange()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "a", "b", "c", "d", "e");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1, new[] { page })
        );

        var first = new WordReference(_book1, 1, 1);
        var last = new WordReference(_book1, 1, 3);

        FeatureState result = FeatureState.SelectWordRangeInEdition(state, first, last);

        Assert.True(result.IsWordSelected(new WordReference(_book1, 1, 1)));
        Assert.True(result.IsWordSelected(new WordReference(_book1, 1, 2)));
        Assert.True(result.IsWordSelected(new WordReference(_book1, 1, 3)));
        Assert.False(result.IsWordSelected(new WordReference(_book1, 1, 0)));
        Assert.False(result.IsWordSelected(new WordReference(_book1, 1, 4)));
    }

    [Fact]
    public void SelectWordRangeInEdition_ReversedOrder_StillSelectsRange()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "a", "b", "c");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1, new[] { page })
        );

        var first = new WordReference(_book1, 1, 2);
        var last = new WordReference(_book1, 1, 0);

        FeatureState result = FeatureState.SelectWordRangeInEdition(state, first, last);

        Assert.True(result.IsWordSelected(new WordReference(_book1, 1, 0)));
        Assert.True(result.IsWordSelected(new WordReference(_book1, 1, 1)));
        Assert.True(result.IsWordSelected(new WordReference(_book1, 1, 2)));
    }

    [Fact]
    public void SelectWordRangeInEdition_DifferentEditions_ReturnsUnchanged()
    {
        FeatureState state = CreateTwoEditionState(["hello"], ["world"]);
        var ref1 = new WordReference(_book1, 1, 0);
        var ref2 = new WordReference(_book2, 1, 0);

        FeatureState result = FeatureState.SelectWordRangeInEdition(state, ref1, ref2);

        Assert.Empty(result.SelectedWords);
    }

    // --- AddWord ---

    [Fact]
    public void AddWord_DelegatesToEditionStateAndUpdatesEditions()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["hello", "world"]);
        var existingRef = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        OcrWord newWord = TestDataBuilder.CreateWord("new");

        FeatureState result = FeatureState.AddWord(state, existingRef, newWord, after: true);

        EditionState edition1 = result.Editions[_book1];
        OcrPage resultPage = edition1.LoadedPages[1].Page;
        Assert.Equal(3, resultPage.Words.Count);
        Assert.Equal("new", resultPage.Words[1]!.GetCombinedText());
        Assert.Equal(_book1, result.LastEditedEdition);
    }

    [Fact]
    public void AddWord_DoesNotAffectOtherEdition()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["foo", "bar"]);
        var existingRef = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        OcrWord newWord = TestDataBuilder.CreateWord("new");

        FeatureState result = FeatureState.AddWord(state, existingRef, newWord, after: true);

        EditionState edition2 = result.Editions[_book2];
        Assert.Equal(2, edition2.LoadedPages[1].Page.Words.Count);
    }

    // --- ReplaceWord ---

    [Fact]
    public void ReplaceWord_DelegatesToEditionStateAndSetsLastEdited()
    {
        FeatureState state = CreateTwoEditionState(["hello", "world"], ["hello", "world"]);
        var wordRef = TestDataBuilder.CreateWordReference(_book1, 1, 0);
        OcrWord replacement = TestDataBuilder.CreateWord("goodbye");

        FeatureState result = FeatureState.ReplaceWord(state, wordRef, [replacement]);

        EditionState edition1 = result.Editions[_book1];
        Assert.Equal("goodbye", edition1.LoadedPages[1].Page.Words[0]!.GetCombinedText());
        Assert.Equal(_book1, result.LastEditedEdition);
    }

    // --- MarkSelectedWordsAsEditorialFormattingChanges ---

    [Fact]
    public void MarkSelectedWordsAsEditorialFormattingChanges_AppliesAndClearsSelection()
    {
        OcrWord word = TestDataBuilder.CreateWord("Hello");
        OcrPage page1 = TestDataBuilder.CreatePageWithWords(1, word);
        OcrPage page2 = TestDataBuilder.CreatePage(1, "hello");

        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1, new[] { page1 }),
            (_book2, new[] { page2 })
        );

        var wordRef = new WordReference(_book1, 1, 0);
        state = FeatureState.SelectWord(state, wordRef);

        FeatureState result = FeatureState.MarkSelectedWordsAsEditorialFormattingChanges(state);

        OcrWord? resultWord = wordRef.GetWord(result.Editions[_book1]);
        Assert.NotNull(resultWord);
        Assert.Equal(BenefitOfDoubt.EditorialFormatting, resultWord.BenefitOfDoubt);
        Assert.Equal("hello", resultWord.BenefitOfDoubtText);
        Assert.Empty(result.SelectedWords);
    }

    // --- SelectWordRangeInEdition across pages ---

    [Fact]
    public void SelectWordRangeInEdition_AcrossPages_SelectsAllWordsInRange()
    {
        OcrPage page1 = TestDataBuilder.CreatePage(1, "a", "b");
        OcrPage page2 = TestDataBuilder.CreatePage(2, "c", "d");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1, new[] { page1, page2 })
        );

        var first = new WordReference(_book1, 1, 1); // "b"
        var last = new WordReference(_book1, 2, 0);  // "c"

        FeatureState result = FeatureState.SelectWordRangeInEdition(state, first, last);

        Assert.True(result.IsWordSelected(new WordReference(_book1, 1, 1)));
        Assert.True(result.IsWordSelected(new WordReference(_book1, 2, 0)));
    }
}
