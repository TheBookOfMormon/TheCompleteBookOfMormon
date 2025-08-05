using DocumentsModel;
using ImageMagick;
using ImageMagick.Drawing;
using static WordsAnalysis.AppLayer.Features.SyncDocuments.PageState;

namespace WordsAnalysis.AppLayer.Extensions;

public static class MagicImageExtensions
{
    public static void ApplyImageOptions(this MagickImage image, ImageOptions? imageOptions)
    {
        if (imageOptions == null || !imageOptions.ShowHighContrast) return;

        image.ColorType = ColorType.Grayscale;
        image.MedianFilter();
        image.Sharpen();
        image.Sharpen();
        image.Contrast();
        if (imageOptions.ApplyThreshold)
        {
            int lower = imageOptions.ThresholdLower;
            int upper = imageOptions.ThresholdUpper;
            if (lower > upper)
                (lower, upper) = (upper, lower);
            image.BlackThreshold(new Percentage(lower));
            image.WhiteThreshold(new Percentage(upper));
        }
        image.ColorType = ColorType.TrueColor;
    }

    public static MagickImage CloneArea(this MagickImage image, OcrRect rect) =>
        new MagickImage(image.CloneArea(rect.X, rect.Y, (uint)Math.Max(1, rect.Width), (uint)Math.Max(1, rect.Height)));

    public static void DrawSplitLine(this MagickImage image, int splitOffset)
    {
        IDrawables<byte> drawables = new Drawables()
            .StrokeColor(MagickColors.Lime)
            .StrokeWidth(1)
            .FillColor(MagickColors.None);
        drawables.Line(splitOffset, 0, splitOffset, image.Height);
        image.Draw(drawables);
    }
}
