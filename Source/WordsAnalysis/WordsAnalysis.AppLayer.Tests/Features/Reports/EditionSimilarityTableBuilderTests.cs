using DocumentsModel;
using WordsAnalysis.AppLayer.Features.Reports;
using WordsAnalysis.AppLayer.Tests.Helpers;

namespace WordsAnalysis.AppLayer.Tests.Features.Reports;

public class EditionSimilarityTableBuilderTests
{
    private readonly OcrBookInfo _book1830 = TestDataBuilder.CreateBookInfo(1830, "Edition1830", "E1");
    private readonly OcrBookInfo _book1837 = TestDataBuilder.CreateBookInfo(1837, "Edition1837", "E2");
    private readonly OcrBookInfo _book1840 = TestDataBuilder.CreateBookInfo(1840, "Edition1840", "E3");

    private static Dictionary<OcrBookInfo, IEnumerable<OcrWord?>> CreateEditions(params (OcrBookInfo book, string[] words)[] editions)
    {
        return editions.ToDictionary(
            e => e.book,
            e => e.words.Select(w => (OcrWord?)TestDataBuilder.CreateWord(w)).AsEnumerable());
    }

    // --- Identical editions ---

    [Fact]
    public void Build_TwoIdenticalEditions_Returns100PercentSimilarity()
    {
        var editions = CreateEditions(
            (_book1830, ["the", "book", "of", "mormon"]),
            (_book1837, ["the", "book", "of", "mormon"]));

        var result = EditionSimilarityTableBuilder.Build(editions);

        // 1837 compared to 1830 should be 100%
        Assert.Equal(100m, result[_book1837][_book1830]);
    }

    // --- Completely different editions ---

    [Fact]
    public void Build_CompletelyDifferentEditions_Returns0PercentSimilarity()
    {
        var editions = CreateEditions(
            (_book1830, ["aaa", "bbb", "ccc"]),
            (_book1837, ["xxx", "yyy", "zzz"]));

        var result = EditionSimilarityTableBuilder.Build(editions);

        Assert.Equal(0m, result[_book1837][_book1830]);
    }

    // --- Case-insensitive matching ---

    [Fact]
    public void Build_CaseDifferences_GetHalfCredit()
    {
        // Exact match = 2 points, case-insensitive match = 1 point
        // All words differ only by case: 3 words * 1 point = 3
        // Max available: 3 words * 2 = 6
        // Score: 3 / 6 * 100 = 50%
        var editions = CreateEditions(
            (_book1830, ["The", "Book", "Of"]),
            (_book1837, ["the", "book", "of"]));

        var result = EditionSimilarityTableBuilder.Build(editions);

        Assert.Equal(50m, result[_book1837][_book1830]);
    }

    // --- Only compares with earlier years ---

    [Fact]
    public void Build_EarliestEdition_HasNoComparisons()
    {
        var editions = CreateEditions(
            (_book1830, ["the", "book"]),
            (_book1837, ["the", "book"]));

        var result = EditionSimilarityTableBuilder.Build(editions);

        // 1830 is the earliest, so it has no entries to compare against
        Assert.Empty(result[_book1830]);
    }

    [Fact]
    public void Build_ThreeEditions_MiddleOnlyComparesToEarliest()
    {
        var editions = CreateEditions(
            (_book1830, ["the", "book"]),
            (_book1837, ["the", "book"]),
            (_book1840, ["the", "book"]));

        var result = EditionSimilarityTableBuilder.Build(editions);

        // 1837 only compares to 1830
        Assert.Single(result[_book1837]);
        Assert.True(result[_book1837].ContainsKey(_book1830));

        // 1840 compares to both 1830 and 1837
        Assert.Equal(2, result[_book1840].Count);
        Assert.True(result[_book1840].ContainsKey(_book1830));
        Assert.True(result[_book1840].ContainsKey(_book1837));
    }

    // --- Null words ---

    [Fact]
    public void Build_NullWordsInEdition_HandledCorrectly()
    {
        var editions = new Dictionary<OcrBookInfo, IEnumerable<OcrWord?>>
        {
            [_book1830] = [TestDataBuilder.CreateWord("the"), null, TestDataBuilder.CreateWord("book")],
            [_book1837] = [TestDataBuilder.CreateWord("the"), null, TestDataBuilder.CreateWord("book")]
        };

        var result = EditionSimilarityTableBuilder.Build(editions);

        // Null words sanitize to "" which is treated as empty (skipped in scoring)
        // 2 matching words * 2 points = 4, maxAvailable = 3 * 2 = 6
        decimal expected = 4m / 6m * 100m;
        Assert.Equal(expected, result[_book1837][_book1830]);
    }

    // --- Mixed exact and case matches ---

    [Fact]
    public void Build_MixedExactAndCaseMatches_CalculatesCorrectly()
    {
        // 2 exact matches (2*2=4) + 1 case-insensitive match (1*1=1) = 5
        // maxAvailable = 3 * 2 = 6
        // Score: 5/6 * 100
        var editions = CreateEditions(
            (_book1830, ["the", "Book", "of"]),
            (_book1837, ["the", "book", "of"]));

        var result = EditionSimilarityTableBuilder.Build(editions);

        decimal expected = 5m / 6m * 100m;
        Assert.Equal(expected, result[_book1837][_book1830]);
    }

    // --- Different length editions ---

    [Fact]
    public void Build_DifferentLengthEditions_ComparesUpToShorterLength()
    {
        // Only first 2 words compared (min length). Both match exactly => 2*2 = 4
        // maxAvailable = 4 * 2 = 8 (4 is the longest edition's word count)
        // Score: 4/8 * 100 = 50%
        var editions = CreateEditions(
            (_book1830, ["the", "book"]),
            (_book1837, ["the", "book", "of", "mormon"]));

        var result = EditionSimilarityTableBuilder.Build(editions);

        Assert.Equal(50m, result[_book1837][_book1830]);
    }

    // --- Single edition ---

    [Fact]
    public void Build_SingleEdition_ReturnsEmptyComparisons()
    {
        var editions = CreateEditions(
            (_book1830, ["the", "book"]));

        var result = EditionSimilarityTableBuilder.Build(editions);

        Assert.Single(result);
        Assert.Empty(result[_book1830]);
    }
}
