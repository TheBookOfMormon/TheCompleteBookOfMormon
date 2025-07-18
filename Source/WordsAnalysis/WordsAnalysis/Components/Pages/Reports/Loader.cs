using ConvertImagesToText;
using DocumentsModel;
using System.Collections.Concurrent;

namespace WordsAnalysis.Components.Pages.Reports;

internal class Loader : EditionsProcessorBase
{
    private readonly ConcurrentDictionary<OcrBookInfo, ConcurrentDictionary<int, OcrPage>> EditionPages = new();

    public Loader(string sourcesDirectoryPath) : base(sourcesDirectoryPath)
    {
    }

    public Dictionary<OcrBookInfo, Dictionary<int, OcrPage>> GetEditions()
    {
        return EditionPages.ToDictionary(x => x.Key, x => x.Value.ToDictionary());
    }

    protected override void ProcessFile(OcrBookInfo bookInfo, string scansDirectoryPath, string scansDeskewedDirectoryPath, string ocrDirectoryPath, string imageFileName, bool multiColumn)
    {
        int pageNumber = int.Parse(Path.GetFileNameWithoutExtension(imageFileName));
        var ocrPage = OcrPage.LoadAsync(this.SourcesDirectoryPath, bookInfo, pageNumber).Result;
        ConcurrentDictionary<int, OcrPage> editionPages = EditionPages.GetOrAdd(bookInfo, x => new ConcurrentDictionary<int, OcrPage>());
        editionPages.TryAdd(pageNumber, ocrPage);
    }
}
