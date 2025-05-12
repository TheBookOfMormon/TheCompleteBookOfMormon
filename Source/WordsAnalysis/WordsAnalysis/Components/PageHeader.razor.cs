using Microsoft.AspNetCore.Components;

namespace WordsAnalysis.Components;
public partial class PageHeader
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter, EditorRequired]
    public required string Href { get; set; }

    [Parameter, EditorRequired]
    public required string Text { get; set; }
}