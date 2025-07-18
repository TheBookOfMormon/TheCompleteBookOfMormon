using DocumentsModel;
using Microsoft.AspNetCore.Components;

namespace WordsAnalysis.Components.Pages.Reports;
public partial class EditionSimilarityTable
{
    [EditorRequired, Parameter]
    public Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>> Data { get; set; } = [];

    private static string GetScoreCellClass(decimal maxScore, decimal score)
    {
        if (maxScore == score) return "--highest-score";
        return "";
    }
}