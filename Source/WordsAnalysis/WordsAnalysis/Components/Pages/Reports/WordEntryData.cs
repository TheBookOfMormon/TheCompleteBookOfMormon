using DocumentsModel;

namespace WordsAnalysis.Components.Pages.Reports;

readonly struct WordEntryData
{
    public required int PageNumber { get; init; }
    public required int WordIndex { get; init; }
    public required OcrWord? Word { get; init; }
}
