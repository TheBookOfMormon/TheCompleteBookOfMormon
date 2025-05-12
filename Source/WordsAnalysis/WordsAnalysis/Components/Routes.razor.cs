using Microsoft.AspNetCore.Components;
using WordsAnalysis.Services;

namespace WordsAnalysis.Components;
public partial class Routes
{
    [Inject]
    private IHtmlService HtmlService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        await HtmlService.InitializeAsync();
    }
}