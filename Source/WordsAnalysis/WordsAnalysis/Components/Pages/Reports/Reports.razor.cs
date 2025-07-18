namespace WordsAnalysis.Components.Pages.Reports;

public partial class Reports : IDisposable
{
    private EditionProcessor EditionProcessor = null!;
    private State CurrentState;
    private readonly Loader Loader;

    private enum State
    {
        Loading,
        DeterminingHierarchy,
        DetectingChanges,
        Finished
    }

    public Reports()
    {
        Loader = new Loader(AppLayer.Constants.Data.SourcesDirectoryPath);
        CurrentState = State.Loading;
    }

    void IDisposable.Dispose()
    {
        Loader.Stop();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            EditionProcessor.StartProcessing();
        }
    }

    private void LoadingFinished()
    {
        CurrentState = State.DeterminingHierarchy;
    }

}