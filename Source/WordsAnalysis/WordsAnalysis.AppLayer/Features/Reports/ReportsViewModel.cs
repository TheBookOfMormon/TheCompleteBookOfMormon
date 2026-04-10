using DocumentsModel;

namespace WordsAnalysis.AppLayer.Features.Reports;

public class ReportsViewModel
{
    public enum ReportsState
    {
        Loading,
        DeterminingHierarchy,
        DetectingChanges,
        Finished
    }

    private readonly Func<Task> stateHasChanged;

    public ReportsState CurrentState { get; private set; }
    public string? DetectChangesStatus { get; private set; }
    public Dictionary<OcrBookInfo, WordEntryData[]>? Editions { get; private set; }
    public EditionHierarchyData HierarchyData { get; private set; } = null!;
    public Loader Loader { get; }
    public Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>> SimilarityTableData { get; private set; } = null!;

    public ReportsViewModel(string sourcesDirectoryPath, Func<Task> stateHasChanged)
    {
        this.stateHasChanged = stateHasChanged;
        Loader = new Loader(sourcesDirectoryPath);
        CurrentState = ReportsState.Loading;
    }

    public void Stop()
    {
        Loader.Stop();
    }

    public async Task LoadingFinishedAsync()
    {
        CurrentState = ReportsState.DeterminingHierarchy;
        await stateHasChanged();
        await Task.Yield();

        Dictionary<OcrBookInfo, Dictionary<int, OcrPage>> editions = Loader.GetEditions();
        Editions = ConvertEditionsData(editions);
        DetermineHierarchy();

        CurrentState = ReportsState.DetectingChanges;
        await stateHasChanged();
        await Task.Yield();
        await DetectChangesAsync(HierarchyData);

        CurrentState = ReportsState.Finished;
    }

    public void DetermineHierarchy()
    {
        Dictionary<OcrBookInfo, IEnumerable<OcrWord?>> editionsWords = Editions!
             .ToDictionary(
                 x => x.Key,
                 x => x.Value.Select(x => x.Word));

        SimilarityTableData = EditionSimilarityTableBuilder.Build(editionsWords);
        HierarchyData = EditionHierarchyDataBuilder.Build(SimilarityTableData);
    }

    public async Task DetectChangesAsync(EditionHierarchyData parent)
    {
        foreach (EditionHierarchyData child in parent.Children)
        {
            await GenerateChangesReportAsync(parent, child);
            await DetectChangesAsync(child);
        }
        DetectChangesStatus = null;
    }

    public async Task GenerateChangesReportAsync(EditionHierarchyData parent, EditionHierarchyData child)
    {
        DetectChangesStatus = $"Detecting changes from {parent.BookInfo.Year} {parent.BookInfo.ShortCode} to {child.BookInfo.Year} {child.BookInfo.ShortCode}";
        await stateHasChanged();
        await Task.Yield();

        WordEntryData[] parentWords = Editions![parent.BookInfo];
        WordEntryData[] childWords = Editions![child.BookInfo];
        using var writer = new StringWriter();
        EditionComparisonDataBuilder.Build(writer, parentWords, childWords);
    }

    public static Dictionary<OcrBookInfo, WordEntryData[]>? ConvertEditionsData(Dictionary<OcrBookInfo, Dictionary<int, OcrPage>> editions)
    {
        var result = new Dictionary<OcrBookInfo, WordEntryData[]>();
        foreach (KeyValuePair<OcrBookInfo, Dictionary<int, OcrPage>> editionKvp in editions)
        {
            var words = new List<WordEntryData>(300000);
            foreach (KeyValuePair<int, OcrPage> pageKvp in editionKvp.Value.OrderBy(x => x.Key))
            {
                int wordIndex = -1;
                foreach (OcrWord? word in pageKvp.Value.Words)
                {
                    wordIndex++;
                    var entry = new WordEntryData {
                        PageNumber = pageKvp.Key,
                        WordIndex = wordIndex,
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
