using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WordsAnalysis.AppLayer.Features.SyncDocuments;

namespace WordsAnalysis.Services;

public interface IHtmlService : IWordGridService
{
    Task CenterImagePointInParent(string elementId, int x, int y);
    Task<bool> FirstColumnHasErrorAsync();
    Task FocusFirstElementAsync(ElementReference container);
    Task InitializeAsync();
    Task ScrollBodyToTopLeftAsync();
    Task<bool> ScrollToNextErrorAsync();
}

sealed class HtmlService : IAsyncDisposable, IHtmlService
{
    private readonly IJSRuntime JS;
    private IJSObjectReference? Module;

    public HtmlService(IJSRuntime js)
    {
        JS = js;
    }


    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (Module != null)
            await Module.DisposeAsync();
    }

    public async Task CenterImagePointInParent(string elementId, int x, int y)
    {
        await Module!.InvokeVoidAsync("centerImagePointInParent", elementId, x, y);
    }

    public async Task InitializeAsync()
    {
        Module = await JS.InvokeAsync<IJSObjectReference>("import", "/site.js");
    }

    public async Task<bool> FirstColumnHasErrorAsync()
    {
        return await Module!.InvokeAsync<bool>("firstColumnHasError");
    }


    public async Task FocusFirstElementAsync(ElementReference container)
    {
        await Module!.InvokeVoidAsync("focusFirstElement", container);
    }

    public async Task<WordGridLocation> GetWordGridLocationAsync()
    {
        return await Module!.InvokeAsync<WordGridLocation>("getWordGridLocation");
    }

    public async Task ScrollBodyToTopLeftAsync()
    {
        await Module!.InvokeVoidAsync("scrollBodyToTopLeft");
    }

    public async Task<bool> ScrollToNextErrorAsync()
    {
        return await Module!.InvokeAsync<bool>("scrollToNextError");
    }
}
