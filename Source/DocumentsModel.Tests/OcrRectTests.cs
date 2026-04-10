using DocumentsModel;

namespace DocumentsModel.Tests;

public class OcrRectTests
{
    private static OcrRect Create(int x, int y, int width, int height) =>
        new OcrRect { X = x, Y = y, Width = width, Height = height };

    // --- Empty sentinel ---

    [Fact]
    public void Empty_HasAllZeroValues()
    {
        Assert.Equal(0, OcrRect.Empty.X);
        Assert.Equal(0, OcrRect.Empty.Y);
        Assert.Equal(0, OcrRect.Empty.Width);
        Assert.Equal(0, OcrRect.Empty.Height);
    }

    // --- GetBottom ---

    [Theory]
    [InlineData(0, 0, 50, 20, 19)]
    [InlineData(10, 5, 100, 30, 34)]
    [InlineData(0, 0, 1, 1, 0)]
    public void GetBottom_ReturnsYPlusHeightMinusOne(int x, int y, int w, int h, int expected)
    {
        Assert.Equal(expected, Create(x, y, w, h).GetBottom());
    }

    // --- GetCenter ---

    [Theory]
    [InlineData(0, 0, 100, 50, 50, 25)]
    [InlineData(10, 20, 40, 60, 30, 50)]
    [InlineData(0, 0, 1, 1, 0, 0)]
    public void GetCenter_ReturnsMiddlePoint(int x, int y, int w, int h, int cx, int cy)
    {
        var (centerX, centerY) = Create(x, y, w, h).GetCenter();
        Assert.Equal(cx, centerX);
        Assert.Equal(cy, centerY);
    }

    // --- GetRight ---

    [Theory]
    [InlineData(0, 0, 50, 20, 49)]
    [InlineData(10, 5, 100, 30, 109)]
    [InlineData(0, 0, 1, 1, 0)]
    public void GetRight_ReturnsXPlusWidthMinusOne(int x, int y, int w, int h, int expected)
    {
        Assert.Equal(expected, Create(x, y, w, h).GetRight());
    }

    // --- IntersectsWith ---

    [Fact]
    public void IntersectsWith_OverlappingRects_ReturnsTrue()
    {
        var a = Create(0, 0, 50, 50);
        var b = Create(25, 25, 50, 50);
        Assert.True(a.IntersectsWith(b));
    }

    [Fact]
    public void IntersectsWith_NonOverlapping_ReturnsFalse()
    {
        var a = Create(0, 0, 10, 10);
        var b = Create(20, 20, 10, 10);
        Assert.False(a.IntersectsWith(b));
    }

    [Fact]
    public void IntersectsWith_TouchingEdges_ReturnsFalse()
    {
        var a = Create(0, 0, 10, 10);
        var b = Create(10, 0, 10, 10);
        Assert.False(a.IntersectsWith(b));
    }

    [Fact]
    public void IntersectsWith_Empty_ReturnsFalse()
    {
        var a = Create(5, 5, 20, 20);
        Assert.False(a.IntersectsWith(OcrRect.Empty));
    }

    // --- IntersectWith ---

    [Fact]
    public void IntersectWith_OverlappingRects_ReturnsIntersection()
    {
        var a = Create(0, 0, 20, 20);
        var b = Create(10, 10, 20, 20);
        var result = a.IntersectWith(b);

        Assert.Equal(10, result.X);
        Assert.Equal(10, result.Y);
        Assert.Equal(10, result.Width);
        Assert.Equal(10, result.Height);
    }

    [Fact]
    public void IntersectWith_NonOverlapping_ReturnsEmpty()
    {
        var a = Create(0, 0, 10, 10);
        var b = Create(20, 20, 10, 10);
        Assert.Equal(OcrRect.Empty, a.IntersectWith(b));
    }

    [Fact]
    public void IntersectWith_ContainedRect_ReturnsInnerRect()
    {
        var outer = Create(0, 0, 100, 100);
        var inner = Create(20, 20, 30, 30);
        var result = outer.IntersectWith(inner);

        Assert.Equal(20, result.X);
        Assert.Equal(20, result.Y);
        Assert.Equal(30, result.Width);
        Assert.Equal(30, result.Height);
    }

    // --- MoveX ---

    [Fact]
    public void MoveX_MovesXAndAdjustsWidth()
    {
        var rect = Create(20, 10, 100, 50);
        var result = rect.MoveX(10);

        Assert.Equal(10, result.X);
        Assert.Equal(110, result.Width);
        Assert.Equal(10, result.Y);
        Assert.Equal(50, result.Height);
    }

    [Fact]
    public void MoveX_MovingRight_ShrinksWidth()
    {
        var rect = Create(10, 0, 100, 50);
        var result = rect.MoveX(30);

        Assert.Equal(30, result.X);
        Assert.Equal(80, result.Width);
    }

    // --- MoveY ---

    [Fact]
    public void MoveY_MovesYAndAdjustsHeight()
    {
        var rect = Create(10, 30, 100, 50);
        var result = rect.MoveY(10);

        Assert.Equal(10, result.Y);
        Assert.Equal(70, result.Height);
        Assert.Equal(10, result.X);
        Assert.Equal(100, result.Width);
    }

    [Fact]
    public void MoveY_MovingDown_ShrinksHeight()
    {
        var rect = Create(0, 10, 50, 100);
        var result = rect.MoveY(30);

        Assert.Equal(30, result.Y);
        Assert.Equal(80, result.Height);
    }

    // --- Normalize ---

    [Fact]
    public void Normalize_NegativeWidth_FlipsToPositive()
    {
        var rect = Create(50, 10, -30, 20);
        var result = rect.Normalize();

        Assert.Equal(20, result.X);
        Assert.Equal(10, result.Y);
        Assert.Equal(30, result.Width);
        Assert.Equal(20, result.Height);
    }

    [Fact]
    public void Normalize_NegativeHeight_FlipsToPositive()
    {
        var rect = Create(10, 50, 30, -20);
        var result = rect.Normalize();

        Assert.Equal(10, result.X);
        Assert.Equal(30, result.Y);
        Assert.Equal(30, result.Width);
        Assert.Equal(20, result.Height);
    }

    [Fact]
    public void Normalize_PositiveValues_Unchanged()
    {
        var rect = Create(10, 20, 30, 40);
        var result = rect.Normalize();

        Assert.Equal(rect, result);
    }

    // --- Offset ---

    [Theory]
    [InlineData(10, 20, 5, 3, 15, 23)]
    [InlineData(0, 0, -5, -3, -5, -3)]
    public void Offset_TranslatesPosition(int x, int y, int dx, int dy, int ex, int ey)
    {
        var rect = Create(x, y, 50, 50);
        var result = rect.Offset(dx, dy);

        Assert.Equal(ex, result.X);
        Assert.Equal(ey, result.Y);
        Assert.Equal(50, result.Width);
        Assert.Equal(50, result.Height);
    }

    // --- ScaleByFactor ---

    [Fact]
    public void ScaleByFactor_DoublesSize_ScalesAroundCenter()
    {
        var rect = Create(100, 100, 100, 100);
        var result = rect.ScaleByFactor(2.0, 2.0);

        Assert.Equal(50, result.X);
        Assert.Equal(50, result.Y);
        Assert.Equal(200, result.Width);
        Assert.Equal(200, result.Height);
    }

    [Fact]
    public void ScaleByFactor_NoChange_ReturnsSameRect()
    {
        var rect = Create(10, 20, 50, 30);
        var result = rect.ScaleByFactor(1.0, 1.0);

        Assert.Equal(rect, result);
    }

    // --- ScaleByPixels ---

    [Fact]
    public void ScaleByPixels_IncreasesSize_CenteredGrowth()
    {
        var rect = Create(100, 100, 100, 100);
        var result = rect.ScaleByPixels(20, 10);

        Assert.Equal(90, result.X);
        Assert.Equal(95, result.Y);
        Assert.Equal(120, result.Width);
        Assert.Equal(110, result.Height);
    }

    [Fact]
    public void ScaleByPixels_ZeroIncrease_ReturnsEquivalent()
    {
        var rect = Create(50, 50, 80, 60);
        var result = rect.ScaleByPixels(0, 0);

        Assert.Equal(rect, result);
    }

    // --- SplitHorizontally ---

    [Fact]
    public void SplitHorizontally_ValidOffset_ReturnsTwoParts()
    {
        var rect = Create(10, 20, 100, 50);
        var (left, right) = rect.SplitHorizontally(40);

        Assert.Equal(10, left.X);
        Assert.Equal(20, left.Y);
        Assert.Equal(40, left.Width);
        Assert.Equal(50, left.Height);

        Assert.Equal(50, right.X);
        Assert.Equal(20, right.Y);
        Assert.Equal(60, right.Width);
        Assert.Equal(50, right.Height);
    }

    [Fact]
    public void SplitHorizontally_OffsetZero_Throws()
    {
        var rect = Create(0, 0, 100, 50);
        Assert.Throws<ArgumentOutOfRangeException>(() => rect.SplitHorizontally(0));
    }

    [Fact]
    public void SplitHorizontally_OffsetNegative_Throws()
    {
        var rect = Create(0, 0, 100, 50);
        Assert.Throws<ArgumentOutOfRangeException>(() => rect.SplitHorizontally(-5));
    }

    [Fact]
    public void SplitHorizontally_OffsetEqualToWidth_Throws()
    {
        var rect = Create(0, 0, 100, 50);
        Assert.Throws<ArgumentOutOfRangeException>(() => rect.SplitHorizontally(100));
    }

    [Fact]
    public void SplitHorizontally_OffsetGreaterThanWidth_Throws()
    {
        var rect = Create(0, 0, 100, 50);
        Assert.Throws<ArgumentOutOfRangeException>(() => rect.SplitHorizontally(150));
    }

    // --- Union ---

    [Fact]
    public void Union_TwoRects_ReturnsBoundingBox()
    {
        var a = Create(10, 20, 30, 40);
        var b = Create(50, 60, 20, 10);
        var result = a.Union(b);

        Assert.Equal(10, result.X);
        Assert.Equal(20, result.Y);
        Assert.Equal(60, result.Width);
        Assert.Equal(50, result.Height);
    }

    [Fact]
    public void Union_ContainedRect_ReturnsOuterRect()
    {
        var outer = Create(0, 0, 100, 100);
        var inner = Create(20, 20, 30, 30);
        var result = outer.Union(inner);

        Assert.Equal(outer, result);
    }

    [Fact]
    public void Union_WithEmpty_ReturnsOriginalBoundingBox()
    {
        var rect = Create(10, 20, 30, 40);
        var result = rect.Union(OcrRect.Empty);

        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(40, result.Width);
        Assert.Equal(60, result.Height);
    }
}
