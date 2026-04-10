using ImageMagick;
using System.Runtime.Caching;

namespace WordsAnalysis.Services;

public interface IImageRepository
{
    MagickImage GetPageImage(string path);
    MagickImage GetFilteredPageImage(string path, Func<MagickImage> getter);
    void SetFilteredPageImage(string path, MagickImage image);
}

internal class ImageRepository : IImageRepository
{
    private readonly MemoryCache Cache;
    private readonly CacheItemPolicy Policy;

    public ImageRepository()
    {
        Cache = MemoryCache.Default;
        Policy = new CacheItemPolicy {
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };
    }

    public MagickImage GetFilteredPageImage(string filePath, Func<MagickImage> getImage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(getImage);

        string key = GetFilteredPageImageKey(filePath);
        return GetImage(key, getImage);
    }

    public MagickImage GetPageImage(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string key = GetPageImageKey(filePath);
        return GetImage(key, () => new MagickImage(filePath));
    }

    public void SetFilteredPageImage(string filePath, MagickImage image)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string key = GetFilteredPageImageKey(filePath);
        _ = SetImage(key, image);
    }

    private string GetFilteredPageImageKey(string filePath) => $"FilteredPageImage:{filePath}";

    public MagickImage GetImage(string key, Func<MagickImage> createImage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(createImage);

        var imageBytes = Cache.Get(key) as IMagickImage<byte>;
        if (imageBytes == null)
        {
            using var image = createImage();
            imageBytes = SetImage(key, image);
        }
        return new MagickImage(imageBytes);
    }

    private string GetPageImageKey(string filePath) => $"PageImage:{filePath}";

    private IMagickImage<byte> SetImage(string key, MagickImage image)
    {
        IMagickImage<byte> imageBytes = image.Clone();
        Cache.Set(key, imageBytes, Policy);
        return imageBytes;
    }

}
