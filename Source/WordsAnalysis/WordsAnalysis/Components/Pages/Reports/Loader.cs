using ConvertImagesToText;
using DocumentsModel;

namespace WordsAnalysis.Components.Pages.Reports;

internal class Loader : EditionsProcessorBase
{
    public Loader(string sourcesDirectoryPath) : base(sourcesDirectoryPath)
    {
    }

    protected override void ProcessFile(OcrBookInfo bookInfo, string scansDirectoryPath, string scansDeskewedDirectoryPath, string ocrDirectoryPath, string imageFileName, bool multiColumn)
    {
        int pageNumber = int.Parse(Path.GetFileNameWithoutExtension(imageFileName));
        var page = OcrPage.LoadAsync(this.SourcesDirectoryPath, bookInfo, pageNumber).Result;
    }
}
