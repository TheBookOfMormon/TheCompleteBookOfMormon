using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Components;

namespace WordsAnalysis.Services;

internal class SyncDocumentsDialogService : ISyncDocumentsDialogService
{
    private readonly IDialogService _dialogService;

    public SyncDocumentsDialogService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public async Task<EditWordDialogResult?> ShowEditWordDialogAsync(EditWordDialogContent content)
    {
        var dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
        IDialogReference dialog = await _dialogService.ShowDialogAsync<EditWordDialog, EditWordDialogContent>(content, dialogParameters);
        DialogResult result = await dialog.Result;
        if (result.Cancelled) return null;
        return (EditWordDialogResult)result.Data!;
    }

    public async Task<DeleteWordsDialogResult?> ShowDeleteWordsDialogAsync(DeleteWordsDialogContent content)
    {
        var dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
        IDialogReference dialog = await _dialogService.ShowDialogAsync<DeleteWordsDialog, DeleteWordsDialogContent>(content, dialogParameters);
        DialogResult result = await dialog.Result;
        if (result.Cancelled) return null;
        return (DeleteWordsDialogResult)result.Data!;
    }

    public async Task<RescanAreaDialogResult?> ShowRescanAreaDialogAsync(RescanAreaDialogContent content)
    {
        var dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
        IDialogReference dialog = await _dialogService.ShowDialogAsync<RescanAreaDialog, RescanAreaDialogContent>(content, dialogParameters);
        DialogResult result = await dialog.Result;
        if (result.Cancelled) return null;
        return (RescanAreaDialogResult)result.Data!;
    }

    public async Task<SplitWordsDialogResult?> ShowSplitWordsDialogAsync(SplitWordsDialogContent content)
    {
        var dialogParameters = new DialogParameters();
        IDialogReference dialog = await _dialogService.ShowDialogAsync<SplitWordsDialog, SplitWordsDialogContent>(content, dialogParameters);
        DialogResult result = await dialog.Result;
        if (result.Cancelled) return null;
        return (SplitWordsDialogResult)result.Data!;
    }

    public async Task ShowViewColumnImagesDialogAsync(ViewColumnImagesDialogContent content)
    {
        var dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
        await _dialogService.ShowDialogAsync<ViewColumnImagesDialog, ViewColumnImagesDialogContent>(content, dialogParameters);
    }
}
