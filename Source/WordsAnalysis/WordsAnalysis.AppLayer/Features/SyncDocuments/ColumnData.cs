using DocumentsModel;
using System.Collections.Immutable;
using System.Globalization;
using WordsAnalysis.AppLayer.Extensions;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public class ColumnData
{
    public required string? MostCommonDisplayText { get; init; }
    public required ColumnDataErrorLevel ErrorLevel { get; init; }

    public static ImmutableArray<ColumnData> FromRowData(ImmutableDictionary<OcrBookInfo, EditionState> editions, ImmutableArray<RowData> rowData)
    {
        int mostWords = rowData.Max(x => x.Words.Count);
        var result = new List<ColumnData>(mostWords);
        for(int columnIndex = 0; columnIndex < mostWords; columnIndex++)
        {
            string?[] wordsInColumn = GetColumnDisplayTexts(editions, rowData, columnIndex).ToArray();
            string[] nonNullWordsInColumn = wordsInColumn.OfType<string>().ToArray();
            string? mostCommonWord = nonNullWordsInColumn.GroupBy(x => x).OrderByDescending(x => x.Count()).FirstOrDefault()?.FirstOrDefault();
            int numberOfUniqueWords = nonNullWordsInColumn.Distinct().Count();
            ColumnDataErrorLevel errorLevel;
            if (nonNullWordsInColumn.Any(x => x == "{min}") || nonNullWordsInColumn.Any(x => x.ToUpper().Contains("CHAPTER")) || nonNullWordsInColumn.Any(x => x.HasCapitalAfterLower()))
                errorLevel = ColumnDataErrorLevel.Error;
            else if (numberOfUniqueWords == 1)
            {
                if (wordsInColumn.Any(x => x is null))
                    errorLevel = ColumnDataErrorLevel.WordAddedOrRemoved;
                else
                    errorLevel = ColumnDataErrorLevel.None;
            }
            else if (nonNullWordsInColumn.Any(x => x.StartsWith("{")))
                errorLevel = ColumnDataErrorLevel.Error;
            else
            {
                numberOfUniqueWords = nonNullWordsInColumn
                    .Select(CultureInfo.InvariantCulture.TextInfo.ToUpper)
                    .OfType<string>().Distinct().Count();
                errorLevel = numberOfUniqueWords switch {
                    0 => ColumnDataErrorLevel.None,
                    1 => ColumnDataErrorLevel.Warning,
                    _ => ColumnDataErrorLevel.Error
                };
            }
            var columnData = new ColumnData { MostCommonDisplayText = mostCommonWord, ErrorLevel = errorLevel };
            result.Add(columnData);
        }
        return result.ToImmutableArray();
    }

    public static ImmutableArray<WordReference?> GetColumnWords(ImmutableDictionary<OcrBookInfo, EditionState> editions, ImmutableArray<RowData> rowData, int columnIndex)
    {
        var result = new WordReference?[rowData.Length];

        for (int rowIndex = 0; rowIndex < rowData.Length; rowIndex++)
        {
            RowData row = rowData[rowIndex];
            if (columnIndex < row.Words.Count && row.Words[columnIndex] is WordReference wordReference)
            {
                result[rowIndex] = wordReference;
            }
        }
        return result.ToImmutableArray();
    }

    public static string?[] GetColumnDisplayTexts(ImmutableDictionary<OcrBookInfo, EditionState> editions, ImmutableArray<RowData> rowData, int columnIndex)
    {
        ImmutableArray<WordReference?> wordReferences = GetColumnWords(editions, rowData, columnIndex);
        return wordReferences.Select(x =>
        {
            if (x == null) return null;
            EditionState editionState = editions[x!.BookInfo];
            return x?.GetWord(editionState)?.GetDisplayText();
        }).ToArray();
    }
}

public enum ColumnDataErrorLevel
{
    None,
    WordAddedOrRemoved,
    Warning,
    Error
}