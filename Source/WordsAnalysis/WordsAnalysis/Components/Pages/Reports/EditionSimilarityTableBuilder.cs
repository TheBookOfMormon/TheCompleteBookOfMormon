using DocumentsModel;

namespace WordsAnalysis.Components.Pages.Reports;

internal static class EditionSimilarityTableBuilder
{
    public static Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>> Build(Dictionary<OcrBookInfo, IEnumerable<OcrWord?>> editions)
    {
        Dictionary<OcrBookInfo, string[]> editionsWords = editions
            .ToDictionary(
                x => x.Key,
                x => x.Value.Select(SanitizeWord).ToArray());
        int maxWords = editionsWords.Max(x => x.Value.Length);
        Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>> similarityTable =
            editionsWords.ToDictionary(
                x => x.Key,
                x => editionsWords
                    .Where(other => other.Key.Year < x.Key.Year)
                    .ToDictionary(x => x.Key, x => 0m));

        int mostWords = editionsWords.Max(x => x.Value.Length);

        foreach(var outer in similarityTable)
        {
            foreach(var inner in outer.Value)
            {
                outer.Value[inner.Key] = GetSimilarityScore(editionsWords[outer.Key], editionsWords[inner.Key], mostWords);
            }
        }

        return similarityTable;
    }

    private static decimal GetSimilarityScore(string[] left, string[] right, int mostWords)
    {
        int maxLength = Math.Min(left.Length, right.Length);
        int result = 0;
        for(int i = 0;  i < maxLength; i++)
        {
            string leftWord = left[i];
            if (!string.IsNullOrEmpty(leftWord))
            {
                string rightWord = right[i];
                if (leftWord.Equals(rightWord, StringComparison.Ordinal))
                {
                    result += 2;
                }
                else if (leftWord.Equals(rightWord, StringComparison.OrdinalIgnoreCase))
                {
                    result += 1;
                }

            }
        }

        decimal maxAvailableScore = mostWords * 2;
        return result / maxAvailableScore * 100m;
    }

    private static string SanitizeWord(OcrWord? word)
    {
        if (word == null) return "";
        if (word.ShowDashes) return "";
        if (word.BenefitOfDoubt == BenefitOfDoubt.InkError) return word.BenefitOfDoubtText!;
        string combinedText = word.GetCombinedText();
        return combinedText;
    }

}
