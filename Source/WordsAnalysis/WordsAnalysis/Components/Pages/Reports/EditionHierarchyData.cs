using DocumentsModel;

namespace WordsAnalysis.Components.Pages.Reports;

public class EditionHierarchyData
{
    public required OcrBookInfo BookInfo { get; init; }
    public List<EditionHierarchyData> Children { get; } = [];
}
