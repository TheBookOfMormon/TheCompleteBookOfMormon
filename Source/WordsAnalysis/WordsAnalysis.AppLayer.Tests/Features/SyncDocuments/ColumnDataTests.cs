using DocumentsModel;
using System.Collections.Immutable;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.AppLayer.Tests.Helpers;

namespace WordsAnalysis.AppLayer.Tests.Features.SyncDocuments;

public class ColumnDataTests
{
    private readonly OcrBookInfo _book1 = TestDataBuilder.CreateBookInfo(1830, "Ed1830", "E1");
    private readonly OcrBookInfo _book2 = TestDataBuilder.CreateBookInfo(1837, "Ed1837", "E2");
    private readonly OcrBookInfo _book3 = TestDataBuilder.CreateBookInfo(1840, "Ed1840", "E3");

    private (ImmutableDictionary<OcrBookInfo, EditionState> Editions, ImmutableArray<RowData> RowData)
        BuildRowData(params (OcrBookInfo book, string[] words)[] editionWords)
    {
        var editions = new Dictionary<OcrBookInfo, EditionState>();
        var rows = new List<RowData>();

        foreach (var (book, words) in editionWords)
        {
            OcrPage page = TestDataBuilder.CreatePage(1, words);
            EditionState edition = TestDataBuilder.CreateEditionState(book, page);
            editions[book] = edition;

            var wordRefs = Enumerable.Range(0, words.Length)
                .Select(i => new WordReference(book, 1, i))
                .ToImmutableList();

            rows.Add(new RowData { BookInfo = book, Words = wordRefs });
        }

        return (editions.ToImmutableDictionary(), rows.ToImmutableArray());
    }

    // --- All same word: ErrorLevel.None ---

    [Fact]
    public void FromRowData_AllSameWord_ReturnsNone()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "hello" }),
            (_book2, new[] { "hello" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.None, result[0].ErrorLevel);
        Assert.Equal("hello", result[0].MostCommonDisplayText);
    }

    [Fact]
    public void FromRowData_AllSameWord_ThreeEditions_ReturnsNone()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "and" }),
            (_book2, new[] { "and" }),
            (_book3, new[] { "and" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.None, result[0].ErrorLevel);
    }

    // --- Same word but some nulls: WordAddedOrRemoved ---

    [Fact]
    public void FromRowData_SameWordWithNullSpacer_ReturnsWordAddedOrRemoved()
    {
        // Edition1 has the word, Edition2 has a null spacer at that position
        OcrPage page1 = TestDataBuilder.CreatePage(1, "hello");
        OcrPage page2 = TestDataBuilder.CreatePageWithWords(1, (OcrWord?)null);

        EditionState edition1 = TestDataBuilder.CreateEditionState(_book1, page1);
        EditionState edition2 = TestDataBuilder.CreateEditionState(_book2, page2);

        var editions = new Dictionary<OcrBookInfo, EditionState>
        {
            [_book1] = edition1,
            [_book2] = edition2
        }.ToImmutableDictionary();

        var rowData = ImmutableArray.Create(
            new RowData { BookInfo = _book1, Words = ImmutableList.Create(new WordReference(_book1, 1, 0)) },
            new RowData { BookInfo = _book2, Words = ImmutableList.Create(new WordReference(_book2, 1, 0)) }
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.WordAddedOrRemoved, result[0].ErrorLevel);
    }

    // --- Words differ only by case: Warning ---

    [Fact]
    public void FromRowData_WordsDifferByCase_ReturnsWarning()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "Hello" }),
            (_book2, new[] { "hello" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.Warning, result[0].ErrorLevel);
    }

    [Fact]
    public void FromRowData_WordsDifferByCaseOnly_ThreeEditions_ReturnsWarning()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "THE" }),
            (_book2, new[] { "the" }),
            (_book3, new[] { "the" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.Warning, result[0].ErrorLevel);
    }

    // --- Words differ in content: Error ---

    [Fact]
    public void FromRowData_DifferentWords_ReturnsError()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "hello" }),
            (_book2, new[] { "world" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.Error, result[0].ErrorLevel);
    }

    // --- {min} flag: Error ---

    [Fact]
    public void FromRowData_MinFlag_ReturnsError()
    {
        // A single-element word "-" renders as "{min}" via GetDisplayText
        OcrWord dashWord = TestDataBuilder.CreateWord("-");
        OcrPage page1 = TestDataBuilder.CreatePageWithWords(1, dashWord);
        OcrPage page2 = TestDataBuilder.CreatePage(1, "hello");

        EditionState edition1 = TestDataBuilder.CreateEditionState(_book1, page1);
        EditionState edition2 = TestDataBuilder.CreateEditionState(_book2, page2);

        var editions = new Dictionary<OcrBookInfo, EditionState>
        {
            [_book1] = edition1,
            [_book2] = edition2
        }.ToImmutableDictionary();

        var rowData = ImmutableArray.Create(
            new RowData { BookInfo = _book1, Words = ImmutableList.Create(new WordReference(_book1, 1, 0)) },
            new RowData { BookInfo = _book2, Words = ImmutableList.Create(new WordReference(_book2, 1, 0)) }
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.Error, result[0].ErrorLevel);
    }

    // --- CHAPTER in word: Error ---

    [Fact]
    public void FromRowData_ChapterWord_ReturnsError()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "CHAPTER" }),
            (_book2, new[] { "CHAPTER" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.Error, result[0].ErrorLevel);
    }

    [Fact]
    public void FromRowData_ChapterInMixedCase_ReturnsError()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "Chapter" }),
            (_book2, new[] { "Chapter" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.Error, result[0].ErrorLevel);
    }

    // --- CamelCase: Error ---

    [Fact]
    public void FromRowData_CamelCase_ReturnsError()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "helloWorld" }),
            (_book2, new[] { "helloWorld" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.Error, result[0].ErrorLevel);
    }

    [Fact]
    public void FromRowData_NoCamelCase_AllUppercase_DoesNotTriggerCamelCaseDetection()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "AND" }),
            (_book2, new[] { "AND" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.None, result[0].ErrorLevel);
    }

    // --- MostCommonDisplayText ---

    [Fact]
    public void FromRowData_MostCommonDisplayText_ReturnsMostFrequent()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "the" }),
            (_book2, new[] { "The" }),
            (_book3, new[] { "the" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal("the", result[0].MostCommonDisplayText);
    }

    // --- Multiple columns ---

    [Fact]
    public void FromRowData_MultipleColumns_EachColumnAnalyzedIndependently()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "hello", "world" }),
            (_book2, new[] { "hello", "earth" })
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Equal(2, result.Length);
        Assert.Equal(ColumnDataErrorLevel.None, result[0].ErrorLevel);
        Assert.Equal(ColumnDataErrorLevel.Error, result[1].ErrorLevel);
    }

    // --- GetColumnWords ---

    [Fact]
    public void GetColumnWords_ReturnsWordReferencesForColumn()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "hello", "world" }),
            (_book2, new[] { "hello", "earth" })
        );

        ImmutableArray<WordReference?> column0 = ColumnData.GetColumnWords(editions, rowData, 0);

        Assert.Equal(2, column0.Length);
        Assert.NotNull(column0[0]);
        Assert.NotNull(column0[1]);
        Assert.Equal(0, column0[0]!.WordIndex);
        Assert.Equal(0, column0[1]!.WordIndex);
    }

    [Fact]
    public void GetColumnWords_ColumnBeyondRowLength_ReturnsNulls()
    {
        var (editions, rowData) = BuildRowData(
            (_book1, new[] { "hello", "world" }),
            (_book2, new[] { "hello" })
        );

        ImmutableArray<WordReference?> column1 = ColumnData.GetColumnWords(editions, rowData, 1);

        Assert.Equal(2, column1.Length);
        Assert.NotNull(column1[0]);
        Assert.Null(column1[1]);
    }

    // --- Curly brace prefix (non-min) triggers Error ---

    [Fact]
    public void FromRowData_CurlyBracePrefix_ReturnsError()
    {
        // A word like ";" renders as "{semi}" via GetDisplayText
        OcrWord semiWord = TestDataBuilder.CreateWord(";");
        OcrPage page1 = TestDataBuilder.CreatePageWithWords(1, semiWord);
        OcrPage page2 = TestDataBuilder.CreatePage(1, "hello");

        EditionState edition1 = TestDataBuilder.CreateEditionState(_book1, page1);
        EditionState edition2 = TestDataBuilder.CreateEditionState(_book2, page2);

        var editions = new Dictionary<OcrBookInfo, EditionState>
        {
            [_book1] = edition1,
            [_book2] = edition2
        }.ToImmutableDictionary();

        var rowData = ImmutableArray.Create(
            new RowData { BookInfo = _book1, Words = ImmutableList.Create(new WordReference(_book1, 1, 0)) },
            new RowData { BookInfo = _book2, Words = ImmutableList.Create(new WordReference(_book2, 1, 0)) }
        );

        ImmutableArray<ColumnData> result = ColumnData.FromRowData(editions, rowData, showBenefitOfDoubt: false);

        Assert.Single(result);
        Assert.Equal(ColumnDataErrorLevel.Error, result[0].ErrorLevel);
    }
}
