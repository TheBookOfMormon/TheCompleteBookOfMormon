using DocumentsModel;
using ImageMagick;
using ImageMagick.Drawing;
using WordsAnalysis.AppLayer.Extensions;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public record PageState
{
    public int AverageLineHeight { get; }
    public Guid ContentsVersion = Guid.NewGuid();
    public OcrPage Page { get; }

    private static readonly Percentage StrokeOpacity = new Percentage(50);

    public PageState(OcrPage page)
    {
        Page = page;
        AverageLineHeight = !page.Words.Any() ? 0 : (int)page.Words.Where(x => x != null).SelectMany(x => x!.Elements).Average(x => x.Bounds.Height);
    }

    public OcrRect GetLineBounds(OcrWord word)
    {
        OcrElement firstElement = word.Elements[0];
        OcrRect firstElementBounds = firstElement.Bounds;
        OcrRect bounds = new OcrRect { X = 0, Y = firstElementBounds.Y, Width = int.MaxValue, Height = firstElementBounds.Height };

        OcrRect[] elementRects = Page.Words.OfType<OcrWord>().SelectMany(x => x.Elements).Select(x => x.Bounds).Where(x => x.IntersectsWith(bounds)).ToArray();
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxRight = firstElement.Bounds.GetRight();
        int maxBottom = firstElement.Bounds.GetBottom();
        foreach (OcrRect elementRect in elementRects)
        {
            if (elementRect.X < minX) minX = elementRect.X;
            if (elementRect.Y < minY) minY = elementRect.Y;

            int right = elementRect.GetRight();
            if (right > maxRight) maxRight = right;

            int bottom = elementRect.GetBottom();
            if (bottom > maxBottom) maxBottom = bottom;
        }
        if (minX >= maxRight && minY >= maxBottom) return OcrRect.Empty;

        return new OcrRect { X = minX, Y = minY, Width = maxRight - minX + 1, Height = maxBottom - minY + 1 };
    }

    public MagickImage GetWordImage(MagickImage image, OcrWord word, bool showSurroundingText, ImageOptions? imageOptions)
    {
        if (showSurroundingText)
            return GetImageForWordAndSurrounding(image, word, imageOptions);
        return GetImageForWordOnly(image, word, imageOptions);
    }

    private static void AddDrawRect(IDrawables<byte> rectangleDrawables, int x, int y, OcrRect elementBounds)
    {
        rectangleDrawables.Rectangle(x, y, x + elementBounds.Width - 1, y + elementBounds.Height - 1);
    }

    private MagickImage GetImageForWordAndSurrounding(MagickImage image, OcrWord word, ImageOptions? imageOptions)
    {
        OcrRect lineBounds = GetLineBounds(word);
        OcrRect wordBounds = word.IsComposite() && !word.Elements[2].IsOnNextPage ? word.Elements[0].Bounds.Union(word.Elements[1].Bounds).Union(word.Elements[2].Bounds) : word.Elements[0].Bounds;
        if (lineBounds == OcrRect.Empty)
        {
            lineBounds = wordBounds with {
                X = 0,
                Width = (int)image.Width
            };
            if (lineBounds.Height == 0)
                lineBounds = lineBounds with { Height = 100 };
        }
        lineBounds = lineBounds.Union(wordBounds);
        lineBounds = lineBounds with {
            X = 0,
            Width = (int)image.Width,
            Y = lineBounds.Y - (AverageLineHeight),
            Height = lineBounds.Height + (AverageLineHeight * 2)
        };

        MagickImage result = image.CloneArea(lineBounds);
        ApplyImageOptions(result, imageOptions);

        IDrawables<byte> rectangleDrawables = new Drawables()
            .FillColor(MagickColors.Lime)
            .FillOpacity(new Percentage(50));

        foreach (var element in word.Elements)
        {
            OcrRect elementBounds = element.Bounds;
            int x = elementBounds.X - lineBounds.X;
            int y = elementBounds.Y - lineBounds.Y;
            if (!element.IsOnNextPage)
                AddDrawRect(rectangleDrawables, x, y, elementBounds);
        }
        rectangleDrawables.Draw(result);

        return result;
    }

    public static MagickImage GetImageForWordOnly(MagickImage image, OcrWord word, ImageOptions? imageOptions)
    {
        const float scale = 1f;

        var originalElements = word.Elements.Where(x => !x.IsOnNextPage).ToList();
        var scaledElements = originalElements.Select(x => x with { Bounds = x.Bounds.ScaleByFactor(scale, scale) }).ToList();
        int maxHeight = scaledElements.Max(x => x.Bounds.Height);
        int totalWidth = scaledElements.Sum(x => x.Bounds.Width);
        var result = new MagickImage(MagickColors.White, (uint)totalWidth, (uint)maxHeight);
        var rectangleDrawables = new Drawables().FillColor(MagickColors.Lime).FillOpacity(new Percentage(50));

        int offset = 0;
        int top = scaledElements[0].Bounds.Y;
        for (int i = 0; i < scaledElements.Count; i++)
        {
            var scaled = scaledElements[i];
            using var elementImage = image.CloneArea(scaled.Bounds);
            int y = (int)result.Height / 2 - scaled.Bounds.Height / 2;
            result.Composite(elementImage, offset, y, CompositeOperator.Over);

            offset += scaled.Bounds.Width;
        }
        ApplyImageOptions(result, imageOptions);
        return result;
    }

    private static void ApplyImageOptions(MagickImage image, ImageOptions? imageOptions)
    {
        if (imageOptions == null || !imageOptions.ShowHighContrast) return;

        image.ColorType = ColorType.Grayscale;
        image.Contrast();
        image.Sharpen();
        image.Sharpen();
        image.MedianFilter();
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

    public class ImageOptions
    {
        public bool ShowHighContrast { get; set; }
        public bool ApplyThreshold { get; set; }
        public int ThresholdLower { get; set; }
        public int ThresholdUpper { get; set; }
    }
}
