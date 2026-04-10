using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Services;

namespace WordsAnalysis.Services;

internal class NotificationService : INotificationService
{
    private readonly IToastService _toastService;

    public NotificationService(IToastService toastService)
    {
        _toastService = toastService;
    }

    public void ShowError(string message, int? timeoutMs = null)
    {
        _toastService.ShowError(message, timeout: timeoutMs ?? 3000);
    }

    public void ShowWarning(string message, int? timeoutMs = null)
    {
        _toastService.ShowWarning(message, timeout: timeoutMs ?? 3000);
    }

    public void ClearAll()
    {
        _toastService.ClearAll();
    }
}
