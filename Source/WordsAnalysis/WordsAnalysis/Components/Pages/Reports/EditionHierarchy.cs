using DocumentsModel;
using System.Collections.Immutable;

namespace WordsAnalysis.Components.Pages.Reports;

internal record class EditionHierarchy
{
    public required OcrBookInfo BookInfo { get; init; }
    public required ImmutableArray<OcrPage> Pages { get; init; }
    public required ImmutableList<EditionHierarchy> Children { get; init; }
}
