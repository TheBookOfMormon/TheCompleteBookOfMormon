using DocumentsModel;

namespace WordsAnalysis.Components.Pages.Reports;

public partial class Reports : IDisposable
{
    private State CurrentState;
    private EditionProcessor EditionProcessor = null!;
    private readonly Loader Loader;
    Dictionary<OcrBookInfo, Dictionary<int, OcrPage>>? Editions;

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
        Editions = Loader.GetEditions();
        StateHasChanged();
        await Task.Yield();
        var editionsWords = Editions
             .ToDictionary(
                 x => x.Key,
                 x => x.Value.OrderBy(x => x.Key).SelectMany(x => x.Value.Words).Select(x => SanitizeWord(x)).ToArray());

        Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>> similarityTable = EditionSimilarityTableBuilder.Build(editionsWords);
    }

    private static string SanitizeWord(OcrWord? word)
    {
        if (word == null) return "";
        if (word.ShowDashes) return "";
        string combinedText = word.GetCombinedText();
        return combinedText;
    }


}