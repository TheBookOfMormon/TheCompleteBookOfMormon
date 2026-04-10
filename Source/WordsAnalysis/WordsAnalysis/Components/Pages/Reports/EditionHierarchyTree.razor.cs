using Microsoft.AspNetCore.Components;
using WordsAnalysis.AppLayer.Features.Reports;

namespace WordsAnalysis.Components.Pages.Reports;
public partial class EditionHierarchyTree
{
    [EditorRequired, Parameter]
    public EditionHierarchyData Data { get; set; } = null!;
}