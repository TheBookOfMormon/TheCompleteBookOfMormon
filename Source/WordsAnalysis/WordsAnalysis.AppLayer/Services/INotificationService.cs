namespace WordsAnalysis.AppLayer.Services;

public interface INotificationService
{
    void ShowError(string message, int? timeoutMs = null);
    void ShowWarning(string message, int? timeoutMs = null);
    void ClearAll();
}
