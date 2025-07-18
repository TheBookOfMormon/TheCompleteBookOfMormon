using Microsoft.AspNetCore.Components;

namespace WordsAnalysis.Components.Pages.Reports;
public partial class EditionHierarchyTree
{
    [EditorRequired, Parameter]
    public EditionHierarchyData Data { get; set; } = null!;
}