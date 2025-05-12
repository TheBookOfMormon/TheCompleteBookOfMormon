using ConvertImagesToText;

namespace WordsAnalysis.Components.Pages.ScanPages;

public partial class Index : IDisposable
{
    private EditionProcessor EditionProcessor = null!;
    private readonly OcrProcessor? OcrProcessor;

    public Index()
    {
        OcrProcessor = new OcrProcessor(AppLayer.Constants.Data.SourcesDirectoryPath);
    }

    void IDisposable.Dispose()
    {
        OcrProcessor?.Stop();
    }
}