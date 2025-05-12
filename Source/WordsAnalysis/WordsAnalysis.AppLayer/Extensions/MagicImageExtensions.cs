using DocumentsModel;
using ImageMagick;
using ImageMagick.Drawing;

namespace WordsAnalysis.AppLayer.Extensions;

public static class MagicImageExtensions
{
    public static MagickImage CloneArea(this MagickImage image, OcrRect rect) =>
        new MagickImage(image.CloneArea(rect.X, rect.Y, (uint)rect.Width, (uint)rect.Height));

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
