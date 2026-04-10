using DocumentsModel;
using System.Collections.Immutable;
using WordsAnalysis.AppLayer.Features.SyncDocuments;

namespace WordsAnalysis.AppLayer.Tests.Helpers;

public static class TestDataBuilder
{
    private static readonly OcrRect DefaultBounds = new OcrRect { X = 0, Y = 0, Width = 50, Height = 20 };

    public static OcrBookInfo CreateBookInfo(int year = 1830, string code = "TestEdition", string shortCode = "TE")
    {
        return new OcrBookInfo {
            Year = year,
            Code = code,
            Name = $"Test Edition {year}",
            ShortCode = shortCode
        };
    }

    public static OcrElement CreateElement(string text = "word", int x = 0, int y = 0, int width = 50, int height = 20, bool isOnNextPage = false)
    {
        return new OcrElement {
            Text = text,
            Bounds = new OcrRect { X = x, Y = y, Width = width, Height = height },
            IsOnNextPage = isOnNextPage
        };
    }

    public static OcrWord CreateWord(string text, int x = 0, int y = 0, int width = 50, int height = 20)
    {
        return new OcrWord {
            Elements = [CreateElement(text, x, y, width, height)]
        };
    }

    public static OcrWord CreateCompositeWord(string part1, string part2, string separator = "-")
    {
        return new OcrWord {
            Elements = [
                CreateElement(part1, 0, 0, 40, 20),
                CreateElement(separator, 40, 0, 10, 20),
                CreateElement(part2, 50, 0, 40, 20)
            ]
        };
    }

    public static OcrPage CreatePage(int pageNumber, params string[] wordTexts)
    {
        var words = wordTexts.Select((text, i) =>
            (OcrWord?)CreateWord(text, x: i * 60, y: 0, width: 50, height: 20)
        ).ToImmutableList();

        return new OcrPage {
            PageNumber = pageNumber,
            ImageWidth = 1000,
            ImageHeight = 800,
            Words = words
        };
    }

    public static OcrPage CreatePageWithWords(int pageNumber, params OcrWord?[] words)
    {
        return new OcrPage {
            PageNumber = pageNumber,
            ImageWidth = 1000,
            ImageHeight = 800,
            Words = words.ToImmutableList()
        };
    }

    public static OcrPageMeta CreatePageMeta(int pageNumber, int numberOfWords)
    {
        return new OcrPageMeta {
            PageNumber = pageNumber,
            NumberOfWords = numberOfWords
        };
    }

    public static EditionState CreateEditionState(OcrBookInfo bookInfo, params OcrPage[] pages)
    {
        var pageMetas = pages.Select(p => CreatePageMeta(p.PageNumber, p.Words.Count));
        var state = new EditionState(bookInfo, pageMetas);

        // Load pages into the state by using reflection or the with expression
        var loadedPages = pages.ToImmutableDictionary(
            p => p.PageNumber,
            p => new PageState(p));

        // Use record's with to set LoadedPages
        return state with { LoadedPages = loadedPages };
    }

    public static FeatureState CreateFeatureState(params (OcrBookInfo BookInfo, OcrPage[] Pages)[] editions)
    {
        var editionStates = editions.Select(e => CreateEditionState(e.BookInfo, e.Pages));
        return new FeatureState {
            SectionIndex = 0,
            Editions = editionStates.ToImmutableDictionary(e => e.BookInfo)
        };
    }

    public static WordReference CreateWordReference(OcrBookInfo bookInfo, int pageNumber = 1, int wordIndex = 0)
    {
        return new WordReference(bookInfo, pageNumber, wordIndex);
    }
}
