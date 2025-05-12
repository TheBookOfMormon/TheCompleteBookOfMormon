using DocumentsModel;
using ImageMagick;

namespace ConvertImagesToText;

public class OcrImageGenerator : EditionsProcessorBase
{
    public OcrImageGenerator(string sourcesDir) : base(sourcesDir) { }

    protected override void ProcessFile(OcrBookInfo bookInfo, string scansDir, string deskewedDir, string ocrDir, string imageFileName, bool multiColumn)
    {
        string imageFilePath = Path.Combine(scansDir, imageFileName);
        string deskewedFileName = Path.ChangeExtension(imageFileName, ".tif");
        string deskewedFilePath = Path.Combine(deskewedDir, deskewedFileName);

        int pageNumber = int.Parse(Path.GetFileNameWithoutExtension(imageFileName));
        bool isPageExcluded = bookInfo.ExcludedPages.Any(x => pageNumber >= x.First && pageNumber <= x.Last);

        if (!File.Exists(deskewedFilePath))
        {
            using var scannedImage = new MagickImage(imageFilePath);
            if (!isPageExcluded)
                scannedImage.Deskew(new Percentage(40));

            scannedImage.Quality = 100;
            scannedImage.Write(deskewedFilePath);
        }
    }
}
