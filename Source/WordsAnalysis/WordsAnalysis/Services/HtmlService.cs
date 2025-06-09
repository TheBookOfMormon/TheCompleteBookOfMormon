using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WordsAnalysis.Services;

public readonly record struct WordGridLocation(int RowIndex, int ColumnIndex)
{
    public readonly static WordGridLocation None = new WordGridLocation(-1, -1);
}

public interface IHtmlService
{
    Task<bool> FirstColumnHasErrorOrWarningAsync();
    Task FocusFirstElementAsync(ElementReference container);
    Task<WordGridLocation> GetWordGridLocationAsync();
    Task InitializeAsync();
    Task ScrollBodyToTopLeftAsync();
    Task<bool> ScrollToNextWarningOrErrorAsync();
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

    public async Task InitializeAsync()
    {
        Module = await JS.InvokeAsync<IJSObjectReference>("import", "/site.js");
    }

    public async Task<bool> FirstColumnHasErrorOrWarningAsync()
    {
        return await Module!.InvokeAsync<bool>("firstColumnHasErrorOrWarning");
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

    public async Task<bool> ScrollToNextWarningOrErrorAsync()
    {
        return await Module!.InvokeAsync<bool>("scrollToNextWarningOrError");
    }
}