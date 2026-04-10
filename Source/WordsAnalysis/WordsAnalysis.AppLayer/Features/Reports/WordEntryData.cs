using DocumentsModel;

namespace WordsAnalysis.AppLayer.Features.Reports;

public readonly struct WordEntryData
{
    public required int PageNumber { get; init; }
    public required int WordIndex { get; init; }
    public required OcrWord? Word { get; init; }
}
