using WordsAnalysis.AppLayer.Features.Reports;

namespace WordsAnalysis.Components.Pages.Reports;

public partial class Reports : IDisposable
{
    private EditionProcessor EditionProcessor = null!;

    public ReportsViewModel ViewModel { get; }

    public Reports()
    {
        ViewModel = new ReportsViewModel(
            AppLayer.Constants.Data.SourcesDirectoryPath,
            async () => { StateHasChanged(); await Task.CompletedTask; });
    }

    void IDisposable.Dispose()
    {
        ViewModel.Stop();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            EditionProcessor.StartProcessing();
        }
    }
}
