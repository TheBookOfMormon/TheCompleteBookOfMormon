using DocumentsModel;

namespace WordsAnalysis.Components.Pages.Reports;

public partial class Reports : IDisposable
{
    private State CurrentState;
    private string? DetectChangesStatus;
    private EditionProcessor EditionProcessor = null!;
    private Dictionary<OcrBookInfo, WordEntry[]>? Editions;
    private EditionHierarchyData HierarchyData = null!;
    private readonly Loader Loader;
    private Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>> SimilarityTableData = null!;

    private readonly struct WordEntry
    {
        public required int PageNumber { get; init; }
        public required int WordIndex { get; init; }
        public required OcrWord? Word { get; init; }
    }

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

        Dictionary<OcrBookInfo, Dictionary<int, OcrPage>> editions = Loader.GetEditions();
        Editions = ConvertEditionsData(editions);
        DetermineHierarchy();

        CurrentState = State.DetectingChanges;
        StateHasChanged();
        await Task.Yield();
        await DetectChangesAsync(HierarchyData);

        CurrentState = State.Finished;
    }

    private void DetermineHierarchy()
    {
        Dictionary<OcrBookInfo, IEnumerable<OcrWord?>> editionsWords = Editions!
             .ToDictionary(
                 x => x.Key,
                 x => x.Value.Select(x => x.Word));

        SimilarityTableData = EditionSimilarityTableBuilder.Build(editionsWords);
        HierarchyData = EditionHierarchyDataBuilder.Build(SimilarityTableData);
    }

    private async Task DetectChangesAsync(EditionHierarchyData parent)
    {
        foreach(EditionHierarchyData child in parent.Children)
        {
            await GenerateChangesReportAsync(parent, child);
            await DetectChangesAsync(child);
        }
        DetectChangesStatus = null;
    }

    private async Task GenerateChangesReportAsync(EditionHierarchyData parent, EditionHierarchyData child)
    {
        DetectChangesStatus = $"Detecting changes from {parent.BookInfo.Year} {parent.BookInfo.ShortCode} to {child.BookInfo.Year} {child.BookInfo.ShortCode}";
        StateHasChanged();
        await Task.Yield();
        await Task.Delay(1000);
    }

    private static Dictionary<OcrBookInfo, WordEntry[]>? ConvertEditionsData(Dictionary<OcrBookInfo, Dictionary<int, OcrPage>> editions)
    {
        var result = new Dictionary<OcrBookInfo, WordEntry[]>();
        foreach (var editionKvp in editions)
        {
            var words = new List<WordEntry>(300000);
            foreach(var pageKvp in editionKvp.Value.OrderBy(x => x.Key))
            {
                int wordIndex = 0;
                foreach(var word in pageKvp.Value.Words)
                {
                    var entry = new WordEntry {
                        PageNumber = pageKvp.Key,
                        WordIndex = wordIndex++,
                        Word = word
                    };
                    words.Add(entry);
                }
            }
            result.Add(editionKvp.Key, words.ToArray());
        }
        return result;
    }
}