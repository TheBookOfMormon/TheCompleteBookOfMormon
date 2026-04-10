using DocumentsModel;
using ImageMagick;

namespace WordsAnalysis.Extensions;

static class MagicImageExtensions
{
    public static MagickImage CloneArea(this MagickImage image, OcrRect rect) =>
        new MagickImage(image.CloneArea(rect.X, rect.Y, (uint)Math.Max(1, rect.Width), (uint)Math.Max(1, rect.Height)));

    public static OcrRect ShrinkOcrRectToContents(this MagickImage image, OcrRect bounds, byte threshold)
    {
        var imageBounds = new MagickGeometry(bounds.X, bounds.Y, (uint)bounds.Width, (uint)bounds.Height);
        // Crop the image to the OcrRect region
        using var cropped = (MagickImage)image.Clone();
        cropped.Crop(imageBounds);

        // Convert to grayscale and apply threshold to distinguish text
        cropped.ColorType = ColorType.Grayscale;
        cropped.Threshold(new Percentage(threshold));

        int minX = (int)cropped.Width, minY = (int)cropped.Height, maxX = -1, maxY = -1;

        // Analyze pixels to find non-background areas
        IPixelCollection<byte> pixels = cropped.GetPixels();
        for (int y = 0; y < cropped.Height; y++)
        {
            for (int x = 0; x < cropped.Width; x++)
            {
                IPixel<byte> pixel = pixels.GetPixel(x, y);
                // For a binary image after thresholding, black text pixels are 0
                if (pixel.GetChannel(0) == 0)
                {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        // If no text pixels found, return the original rect
        if (maxX == -1 || maxY == -1)
            return bounds;

        // Calculate new adjusted rectangle relative to the original image
        int adjustedX = imageBounds.X + minX;
        int adjustedY = imageBounds.Y + minY;
        int adjustedWidth = maxX - minX + 1;
        int adjustedHeight = maxY - minY + 1;

        return new OcrRect {
            X = adjustedX,
            Y = adjustedY,
            Width = adjustedWidth,
            Height = adjustedHeight
        };
    }

    public static string? ToEmbeddedHtmlImage(this MagickImage? image)
    {
        if (image == null) return null;

        using var memoryStream = new MemoryStream();
        image.Write(memoryStream, MagickFormat.Jpg);
        var base64String = Convert.ToBase64String(memoryStream.ToArray());
        return $"data:image/jpg;base64,{base64String}";
    }

}
