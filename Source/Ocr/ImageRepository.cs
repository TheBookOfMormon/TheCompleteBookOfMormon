using Microsoft.Extensions.Options;
using Ocr.Config;

namespace Ocr;

internal class ImageRepository
{
    private readonly IOptions<OcrSettings> OcrSettings;

    public ImageRepository(IOptions<OcrSettings> ocrSettings)
    {
        OcrSettings = ocrSettings ?? throw new ArgumentNullException(nameof(ocrSettings));
    }

    public string[] GetRootImageFilePaths() => GetImageFilePaths(OcrSettings.Value.ScanDirectory);

    public string[] GetImageFilePaths(string path)
    {
        var result = new List<string>();
        foreach (string ext in new string[] { "png", "jpeg", "jpg" })
        {
            string[] imagePaths = Directory.GetFiles(
                path: path,
                searchPattern: $"*.{ext}",
                new EnumerationOptions { RecurseSubdirectories = true });
            result.AddRange(imagePaths);
        }
        return result
            .OrderBy(x => x)
            .ToArray();
    }
}
