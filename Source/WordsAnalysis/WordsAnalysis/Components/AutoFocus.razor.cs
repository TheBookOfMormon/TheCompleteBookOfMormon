using Microsoft.AspNetCore.Components;
using WordsAnalysis.Services;

namespace WordsAnalysis.Components;

public partial class AutoFocus
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? ContainerCssClass { get; set; }

    [Parameter]
    public bool Enabled { get; set; } = true;

    [Inject]
    private IHtmlService HtmlService { get; set; } = null!;

    private ElementReference Container;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
            await HtmlService.FocusFirstElementAsync(Container);
    } 
}