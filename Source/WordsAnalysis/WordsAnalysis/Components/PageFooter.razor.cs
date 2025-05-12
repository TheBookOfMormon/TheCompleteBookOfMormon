using Microsoft.AspNetCore.Components;

namespace WordsAnalysis.Components;

public partial class PageFooter
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}