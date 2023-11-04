using Microsoft.Extensions.Options;
using Ocr.Config;

namespace Ocr;

internal class ImageRepository
{
    private readonly IOptions<SourceImagesSettings> SourceImagesSettings;

    public ImageRepository(IOptions<SourceImagesSettings> sourceImagesSettings)
    {
        SourceImagesSettings = sourceImagesSettings ?? throw new ArgumentNullException(nameof(sourceImagesSettings));
    }

    public string[] GetRootImageFilePaths() => GetImageFilePaths(SourceImagesSettings.Value.Directory);

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
