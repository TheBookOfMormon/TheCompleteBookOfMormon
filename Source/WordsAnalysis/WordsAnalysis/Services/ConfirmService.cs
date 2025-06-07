using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.Components;

namespace WordsAnalysis.Services;

public interface IConfirmService
{
    ValueTask<bool> ConfirmAsync(string message);
}

class ConfirmService : IConfirmService
{
    private readonly IDialogService DialogService;

    public ConfirmService(IDialogService dialogService)
    {
        DialogService = dialogService;
    }

    public async ValueTask<bool> ConfirmAsync(string message)
    {
        var dialogContent = new ConfirmDialogContent { Message = message };
        var dialogParameters = new DialogParameters { };
        var dialog = await DialogService.ShowDialogAsync<ConfirmDialog, ConfirmDialogContent>(dialogContent, dialogParameters);

        DialogResult result = await dialog.Result;
        return !result.Cancelled && result.Data is bool confirmed && confirmed;
    }
}
