using ImageMagick;
using System.Runtime.Caching;

namespace WordsAnalysis.Services;

public interface IImageRepository
{
    MagickImage Get(string path);
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
    public MagickImage Get(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var imageBytes = Cache.Get(filePath) as IMagickImage<byte>;
        if (imageBytes == null)
        {
            using var image = new MagickImage(filePath);
            imageBytes = image.Clone();
            Cache.Set(filePath, imageBytes, Policy);
        }
        return new MagickImage(imageBytes);
    }
}
