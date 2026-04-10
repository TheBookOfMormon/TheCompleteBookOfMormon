using DocumentsModel;


namespace DocumentsModel.Tests;

public class OcrWordTests
{
    private static OcrElement CreateElement(string text = "word", int x = 0, int y = 0, int width = 50, int height = 20, bool isOnNextPage = false)
    {
        return new OcrElement {
            Text = text,
            Bounds = new OcrRect { X = x, Y = y, Width = width, Height = height },
            IsOnNextPage = isOnNextPage
        };
    }

    private static OcrWord CreateWord(string text, int x = 0, int y = 0, int width = 50, int height = 20)
    {
        return new OcrWord {
            Elements = [CreateElement(text, x, y, width, height)]
        };
    }

    private static OcrWord CreateCompositeWord(string part1, string part2, string separator = "-")
    {
        return new OcrWord {
            Elements = [
                CreateElement(part1, 0, 0, 40, 20),
                CreateElement(separator, 40, 0, 10, 20),
                CreateElement(part2, 50, 0, 40, 20)
            ]
        };
    }

    // --- GetCombinedText ---

    [Fact]
    public void GetCombinedText_SingleElement_ReturnsText()
    {
        var word = CreateWord("hello");
        Assert.Equal("hello", word.GetCombinedText());
    }

    [Fact]
    public void GetCombinedText_CompositeWord_ShowDashesFalse_SkipsDashesExceptFirst()
    {
        var word = CreateCompositeWord("any", "thing");
        // Elements: "any", "-", "thing". ShowDashes defaults to false.
        // index 0 always included; for index > 0, skip if text == "-"
        Assert.Equal("anything", word.GetCombinedText());
    }

    [Fact]
    public void GetCombinedText_CompositeWord_ShowDashesTrue_IncludesAllDashes()
    {
        var word = CreateCompositeWord("any", "thing") with { ShowDashes = true };
        Assert.Equal("any-thing", word.GetCombinedText());
    }

    [Fact]
    public void GetCombinedText_FirstElementIsDash_AlwaysIncluded()
    {
        // Edge case: first element is a dash
        var word = new OcrWord {
            Elements = [
                CreateElement("-", 0, 0, 10, 20),
                CreateElement("word", 10, 0, 40, 20)
            ]
        };
        Assert.Equal("-word", word.GetCombinedText());
    }

    // --- GetDisplayText ---

    [Fact]
    public void GetDisplayText_NoBenefitOfDoubt_ReturnsDisplayText()
    {
        var word = CreateWord("hello");
        Assert.Equal("hello", word.GetDisplayText(showBenefitOfDoubt: false));
    }

    [Fact]
    public void GetDisplayText_ShowBenefitOfDoubtTrue_WithBenefitOfDoubt_ReturnsBenefitOfDoubtText()
    {
        var word = CreateWord("helo") with {
            BenefitOfDoubt = BenefitOfDoubt.PrinterError,
            BenefitOfDoubtText = "hello"
        };
        Assert.Equal("hello", word.GetDisplayText(showBenefitOfDoubt: true));
    }

    [Fact]
    public void GetDisplayText_ShowBenefitOfDoubtTrue_NullBenefitOfDoubtText_ReturnsEmpty()
    {
        var word = CreateWord("helo") with {
            BenefitOfDoubt = BenefitOfDoubt.InkError,
            BenefitOfDoubtText = null
        };
        Assert.Equal("", word.GetDisplayText(showBenefitOfDoubt: true));
    }

    [Fact]
    public void GetDisplayText_ShowBenefitOfDoubtFalse_IgnoresBenefitOfDoubt()
    {
        var word = CreateWord("helo") with {
            BenefitOfDoubt = BenefitOfDoubt.PrinterError,
            BenefitOfDoubtText = "hello"
        };
        Assert.Equal("helo", word.GetDisplayText(showBenefitOfDoubt: false));
    }

    [Fact]
    public void GetDisplayText_BenefitOfDoubtNone_ShowBenefitOfDoubtTrue_ReturnsDisplayText()
    {
        var word = CreateWord("hello") with {
            BenefitOfDoubt = BenefitOfDoubt.None
        };
        Assert.Equal("hello", word.GetDisplayText(showBenefitOfDoubt: true));
    }

    [Fact]
    public void GetDisplayText_CompositeShowDashesFalse_UsesElementDisplayText()
    {
        // Composite word with special characters: uses GetDisplayText on each element
        var word = new OcrWord {
            Elements = [
                CreateElement("word", 0, 0, 40, 20),
                CreateElement("-", 40, 0, 10, 20),
                CreateElement("end", 50, 0, 30, 20)
            ]
        };
        // ShowDashes=false: skip "-" at non-zero index, use GetDisplayText for each element
        Assert.Equal("wordend", word.GetDisplayText(showBenefitOfDoubt: false));
    }

    [Fact]
    public void GetDisplayText_CompositeShowDashesTrue_UsesRawText()
    {
        var word = new OcrWord {
            Elements = [
                CreateElement("word", 0, 0, 40, 20),
                CreateElement("-", 40, 0, 10, 20),
                CreateElement("end", 50, 0, 30, 20)
            ],
            ShowDashes = true
        };
        // ShowDashes=true: includes all, uses x.Text (raw text)
        Assert.Equal("word-end", word.GetDisplayText(showBenefitOfDoubt: false));
    }

    // --- IsComposite ---

    [Fact]
    public void IsComposite_SingleElement_ReturnsFalse()
    {
        var word = CreateWord("hello");
        Assert.False(word.IsComposite());
    }

    [Fact]
    public void IsComposite_MultipleElements_ReturnsTrue()
    {
        var word = CreateCompositeWord("any", "thing");
        Assert.True(word.IsComposite());
    }

    [Fact]
    public void IsComposite_TwoElements_ReturnsTrue()
    {
        var word = new OcrWord {
            Elements = [
                CreateElement("A"),
                CreateElement("B")
            ]
        };
        Assert.True(word.IsComposite());
    }

    // --- LastElementOnSamePage ---

    [Fact]
    public void LastElementOnSamePage_AllOnSamePage_ReturnsLast()
    {
        var word = CreateCompositeWord("any", "thing");
        var result = word.LastElementOnSamePage();

        Assert.Equal("thing", result.Text);
    }

    [Fact]
    public void LastElementOnSamePage_LastIsOnNextPage_ReturnsSecondToLast()
    {
        var word = new OcrWord {
            Elements = [
                CreateElement("start", 0, 0, 40, 20),
                CreateElement("-", 40, 0, 10, 20),
                CreateElement("end", 50, 0, 40, 20, isOnNextPage: true)
            ]
        };
        var result = word.LastElementOnSamePage();

        Assert.Equal("-", result.Text);
    }

    [Fact]
    public void LastElementOnSamePage_SingleElementOnSamePage_ReturnsThatElement()
    {
        var word = CreateWord("only");
        var result = word.LastElementOnSamePage();

        Assert.Equal("only", result.Text);
    }
}
