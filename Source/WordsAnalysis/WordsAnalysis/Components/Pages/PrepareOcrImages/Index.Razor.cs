using ConvertImagesToText;

namespace WordsAnalysis.Components.Pages.PrepareOcrImages;

public partial class Index
{
    private EditionProcessor EditionProcessor = null!;
    private readonly OcrImageGenerator ImageGenerator;

    public Index()
    {
        ImageGenerator = new OcrImageGenerator(AppLayer.Constants.Data.SourcesDirectoryPath);
    }
}