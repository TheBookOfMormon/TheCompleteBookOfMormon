namespace DocumentsModel;

public record OcrRect
{
    public static readonly OcrRect Empty = new OcrRect { X = 0, Y = 0, Width = 0, Height = 0 };

    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }

    public int GetBottom() => Y + Height - 1;

    public (int X, int Y) GetCenter() => (Width / 2 + X, Height / 2 + Y);

    public int GetRight() => X + Width - 1;

    public bool IntersectsWith(OcrRect other)
    {
        return (X + Width > other.X && other.X + other.Width > X &&
                Y + Height > other.Y && other.Y + other.Height > Y);
    }

    public OcrRect IntersectWith(OcrRect other)
    {
        if (!IntersectsWith(other))
            return Empty;

        int newX = Math.Max(X, other.X);
        int newY = Math.Max(Y, other.Y);
        int newRight = Math.Min(X + Width, other.X + other.Width);
        int newBottom = Math.Min(Y + Height, other.Y + other.Height);

        return new OcrRect { X = newX, Y = newY, Width = newRight - newX, Height = newBottom - newY };
    }

    public OcrRect MoveX(int x) => 
        new OcrRect { 
            X = x,
            Y = Y,
            Width = Width + (X - x),
            Height = Height
        };

    public OcrRect MoveY(int y) =>
        new OcrRect {
            X = X,
            Y = y,
            Width = Width,
            Height = Height + (Y - y)
        };

    public OcrRect Normalize() =>
        new OcrRect {
            X = Width < 0 ? X + Width : X,
            Y = Height < 0 ? Y + Height : Y,
            Width = Math.Abs(Width),
            Height = Math.Abs(Height)
        };

    public OcrRect Offset(int x, int y) => new OcrRect { X = X + x, Y = Y + y, Width = Width, Height = Height };

    public OcrRect ScaleByFactor(double widthFactor, double heightFactor) => ScaleByPixels((int)((widthFactor - 1) * Width), (int)((heightFactor - 1) * Height));

    public OcrRect ScaleByPixels(int widthPixels, double heightPixels)
    {
        int newWidth = (int)(Width + widthPixels);
        int newHeight = (int)(Height + heightPixels);
        int halfOfWidthIncrease = (newWidth - Width) / 2;
        int halfOfHeightIncrease = (newHeight - Height) / 2;
        return new OcrRect { X = X - halfOfWidthIncrease, Y = Y - halfOfHeightIncrease, Width = newWidth, Height = newHeight };
    }

    public (OcrRect left, OcrRect right) SplitHorizontally(int offset)
    {
        if (offset <= 0 || offset >= Width)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be between 1 and the rectangle's width - 1.");

        var left = new OcrRect{ X = X, Y = Y, Width = offset, Height = Height };
        var right = new OcrRect{ X = X + offset, Y = Y, Width = Width - offset, Height = Height };

        return (left, right);
    }


    public OcrRect Union(OcrRect other)
    {
        int x = Math.Min(X, other.X);
        int y = Math.Min(Y, other.Y);
        int right = Math.Max(X + Width, other.X + other.Width);
        int bottom = Math.Max(Y + Height, other.Y + other.Height);
        return new OcrRect{ X = x, Y = y, Width = right - x, Height = bottom - y };
    }

}
