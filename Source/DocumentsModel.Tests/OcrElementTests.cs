using DocumentsModel;


namespace DocumentsModel.Tests;

public class OcrElementTests
{
    private static OcrElement CreateElement(string text = "word", int x = 0, int y = 0, int width = 50, int height = 20, bool isOnNextPage = false)
    {
        return new OcrElement {
            Text = text,
            Bounds = new OcrRect { X = x, Y = y, Width = width, Height = height },
            IsOnNextPage = isOnNextPage
        };
    }

    // --- GetDisplayText: special character mappings ---

    [Theory]
    [InlineData("'", "{apos}")]
    [InlineData("\"", "{quot}")]
    [InlineData("!", "{excl}")]
    [InlineData("-", "{min}")]
    [InlineData(":", "{col}")]
    [InlineData(";", "{semi}")]
    [InlineData(",", "{com}")]
    [InlineData("&", "{amp}")]
    [InlineData(".", "{dot}")]
    [InlineData("\u2014", "{hyph}")]
    [InlineData("(", "{open}")]
    [InlineData(")", "{close}")]
    public void GetDisplayText_SpecialCharacter_ReturnsMappedToken(string text, string expected)
    {
        var element = CreateElement(text);
        Assert.Equal(expected, element.GetDisplayText());
    }

    [Theory]
    [InlineData("word")]
    [InlineData("Hello")]
    [InlineData("123")]
    [InlineData("the")]
    public void GetDisplayText_RegularText_PassesThrough(string text)
    {
        var element = CreateElement(text);
        Assert.Equal(text, element.GetDisplayText());
    }

    [Fact]
    public void GetDisplayText_MultiCharacterText_PassesThrough()
    {
        var element = CreateElement("multi-word");
        Assert.Equal("multi-word", element.GetDisplayText());
    }

    // --- Append ---

    [Fact]
    public void Append_ConcatenatesText()
    {
        var first = CreateElement("Hello", x: 0, y: 0, width: 40, height: 20);
        var second = CreateElement("World", x: 40, y: 0, width: 50, height: 20);
        var result = first.Append(second);

        Assert.Equal("HelloWorld", result.Text);
    }

    [Fact]
    public void Append_UnionsBounds()
    {
        var first = CreateElement("A", x: 10, y: 20, width: 30, height: 15);
        var second = CreateElement("B", x: 50, y: 10, width: 20, height: 30);
        var result = first.Append(second);

        Assert.Equal(10, result.Bounds.X);
        Assert.Equal(10, result.Bounds.Y);
        Assert.Equal(60, result.Bounds.Width);
        Assert.Equal(30, result.Bounds.Height);
    }

    [Fact]
    public void Append_PreservesIsOnNextPageFromOriginal()
    {
        var first = CreateElement("A", isOnNextPage: true);
        var second = CreateElement("B");
        var result = first.Append(second);

        Assert.True(result.IsOnNextPage);
    }

    [Fact]
    public void Append_EmptyTextElements_ConcatenatesToEmptyLikeText()
    {
        var first = CreateElement("");
        var second = CreateElement("text");
        var result = first.Append(second);

        Assert.Equal("text", result.Text);
    }
}
