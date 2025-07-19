using System.Diagnostics;

namespace WordsAnalysis.Components.Pages.Reports;

internal static class EditionComparisonDataBuilder
{
    [DebuggerDisplay("{ParentWordText} -> {ChildWordText} (P{ParentPage} & P{ChildPage})")]
    private class Change
    {
        public required int ParentPage { get; init; }
        public required int ParentWordIndex { get; init; }
        public required string? ParentWordText { get; init; }
        public required int ChildPage { get; init; }
        public required int ChildWordIndex { get; init; }
        public required string? ChildWordText { get; init; }
    }

    public static void Build(
        TextWriter writer,
        WordEntryData[] parentWords,
        WordEntryData[] childWords)
    {
        int maxIndex = Math.Max(parentWords.Length, childWords.Length);
        var changes = new List<Change>(maxIndex);
        WordEntryData previousParentWordData = parentWords[0];
        WordEntryData previousChildWordData = childWords[0];
        for (int i = 0; i < maxIndex; i++)
        {
            WordEntryData parentWordData = GetWordData(parentWords, ref previousParentWordData, i);
            string? parentWordText = parentWordData.Word?.GetDisplayText(showBenefitOfDoubt: true) ?? "";

            WordEntryData childWordData = GetWordData(childWords, ref previousChildWordData, i);
            string? childWordText = childWordData.Word?.GetDisplayText(showBenefitOfDoubt: true) ?? "";

            if (!string.Equals(parentWordText, childWordText, StringComparison.Ordinal))
            {
                var change = new Change {
                    ParentPage = parentWordData.PageNumber,
                    ParentWordIndex = parentWordData.WordIndex,
                    ParentWordText = parentWordText,
                    ChildPage = childWordData.PageNumber,
                    ChildWordIndex = childWordData.WordIndex,
                    ChildWordText = childWordText
                };
                changes.Add(change);
            }
        }
    }

    private static WordEntryData GetWordData(WordEntryData[] words, ref WordEntryData previousWordData, int wordIndex)
    {
        WordEntryData result;
        if (wordIndex < words.Length)
            result = words[wordIndex];
        else
            result = new WordEntryData {
                PageNumber = previousWordData.PageNumber,
                WordIndex = wordIndex,
                Word = null
            };
        previousWordData = result;
        return result;
    }
}
