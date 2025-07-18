using DocumentsModel;

namespace WordsAnalysis.Components.Pages.Reports;

public partial class Reports : IDisposable
{
    private State CurrentState;
    private EditionProcessor EditionProcessor = null!;
    private Dictionary<OcrBookInfo, Dictionary<int, OcrPage>>? Editions;
    private readonly Loader Loader;
    private Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>> SimilarityTableData = null!;

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

    private async Task LoadingFinishedAsync()
    {
        CurrentState = State.DeterminingHierarchy;
        StateHasChanged();
        await Task.Yield();

        Editions = Loader.GetEditions();
        await DetermineHierarchyAsync();

        CurrentState = State.Finished;
        StateHasChanged();
        await Task.Yield();

    }

    private Task DetermineHierarchyAsync()
    {
        Dictionary<OcrBookInfo, IEnumerable<OcrWord?>> editionsWords = Editions!
             .ToDictionary(
                 x => x.Key,
                 x => x.Value.OrderBy(x => x.Key).SelectMany(x => x.Value.Words));

        SimilarityTableData = EditionSimilarityTableBuilder.Build(editionsWords);
        return Task.CompletedTask;
    }
}