using DocumentsModel;
using System.Collections.Immutable;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public record class RowData
{
    public required OcrBookInfo BookInfo { get; init; }
    public required ImmutableList<WordReference> Words { get; init; }
}
