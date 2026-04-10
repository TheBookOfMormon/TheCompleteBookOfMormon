using DocumentsModel;

namespace WordsAnalysis.AppLayer.Features.Reports;

public class EditionHierarchyData
{
    public required OcrBookInfo BookInfo { get; init; }
    public List<EditionHierarchyData> Children { get; } = [];
}
