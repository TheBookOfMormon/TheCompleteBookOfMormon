using System.Collections.Immutable;
using System.Text.Json;
using DocumentsModel;
using ImageMagick;
using Tesseract;

namespace ConvertImagesToText;

public partial class OcrProcessor : EditionsProcessorBase
{
    private const string AllowedScanChars = "1234567890 abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ\"-–—.,;:'()*&?!/";
    private const string AllowedChars = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-'&";
    private const int DefaultUpscaleFactor = 3;

    private static readonly Dictionary<char, double> LetterHeightToWidthFactors = new Dictionary<char, double>
    {
        { 'A', 0.79d },
        { 'B', 0.75d },
        { 'C', 0.75d },
        { 'D', 0.79d },
        { 'E', 0.67d },
        { 'F', 0.62d },
        { 'G', 0.79d },
        { 'H', 0.83d },
        { 'I', 0.52d },
        { 'J', 0.54d },
        { 'K', 0.79d },
        { 'L', 0.62d },
        { 'M', 1.00d },
        { 'N', 0.83d },
        { 'O', 0.79d },
        { 'P', 0.75d },
        { 'Q', 0.79d },
        { 'R', 0.79d },
        { 'S', 0.71d },
        { 'T', 0.71d },
        { 'U', 0.83d },
        { 'V', 0.79d },
        { 'W', 1.17d },
        { 'X', 0.79d },
        { 'Y', 0.79d },
        { 'Z', 0.75d },
        { 'a', 0.67d },
        { 'b', 0.71d },
        { 'c', 0.67d },
        { 'd', 0.71d },
        { 'e', 0.67d },
        { 'f', 0.42d },
        { 'g', 0.71d },
        { 'h', 0.71d },
        { 'i', 0.38d },
        { 'j', 0.38d },
        { 'k', 0.67d },
        { 'l', 0.38d },
        { 'm', 1.00d },
        { 'n', 0.71d },
        { 'o', 0.67d },
        { 'p', 0.71d },
        { 'q', 0.71d },
        { 'r', 0.46d },
        { 's', 0.67d },
        { 't', 0.42d },
        { 'u', 0.71d },
        { 'v', 0.67d },
        { 'w', 0.96d },
        { 'x', 0.67d },
        { 'y', 0.67d },
        { 'z', 0.67d },
    };

    public OcrProcessor(string sourcesDirectoryPath) : base(sourcesDirectoryPath)
    {
    }

    public OcrWord[] ProcessImage(OcrBookInfo bookInfo, MagickImage sourceImage, bool multiColumn, int upscaleFactor = DefaultUpscaleFactor)
    {
        bool wasProcessing = IsProcessing;
        IsProcessing = true;
        try
        {
            return ProcessImageInternal(bookInfo, sourceImage, upscaleFactor);
        }
        finally
        {
            IsProcessing = wasProcessing;
        }
    }

    protected override void ProcessFile(OcrBookInfo bookInfo, string scansDirectoryPath, string scansDeskewedDirectoryPath, string ocrDirectoryPath, string imageFileName, bool multiColumn)
    {
        if (!IsProcessing) return;

        string imageFilePath = Path.Combine(scansDirectoryPath, imageFileName);
        string deskewedImageFileName = Path.ChangeExtension(imageFileName, ".tif");
        string deskewedImageFilePath = Path.Combine(scansDeskewedDirectoryPath, deskewedImageFileName);

        string ocrFileName = Path.ChangeExtension(deskewedImageFileName, Constants.PageFileNameExtension);
        string ocrFilePath = Path.Combine(ocrDirectoryPath, ocrFileName);
        string ocrMetaFilePath = Path.ChangeExtension(ocrFilePath, Constants.PageMetaFileNameExtension);

        int pageNumber = int.Parse(Path.GetFileNameWithoutExtension(imageFileName));
        bool isPageExcluded = getIsPageExcluded(pageNumber);

        if (File.Exists(ocrFilePath) && !File.Exists(deskewedImageFilePath))
        {
            File.Delete(ocrFilePath);
            File.Delete(ocrMetaFilePath);
            return;
        }

        if (File.Exists(ocrFilePath))
        {
            if (File.Exists(ocrMetaFilePath)) return;

            // Delete files if unmodified
            string existingJson = File.ReadAllText(ocrFilePath);
            var existingPage = JsonSerializer.Deserialize<OcrPage>(existingJson)!;
            if (existingPage.ManuallyEdited)
            {
                if (!File.Exists(ocrMetaFilePath))
                    writeMeta(existingPage);
                return;
            }
            else
            {
                File.Delete(ocrFilePath);
                if (File.Exists(ocrMetaFilePath))
                    File.Delete(ocrMetaFilePath);
            }
        }

        int imageWidth = 0;
        int imageHeight = 0;
        OcrWord[] words = [];

        if (!isPageExcluded)
        {
            using var image = new MagickImage(deskewedImageFilePath);
            imageWidth = (int)image.Width;
            imageHeight = (int)image.Height;
            words = ProcessImageInternal(bookInfo, image);
        }

        if (IsProcessing)
        {
            var ocrPage = new OcrPage {
                PageNumber = pageNumber,
                Words = words.Cast<OcrWord?>().ToImmutableList(),
                ImageHeight = imageHeight,
                ImageWidth = imageWidth
            };
            string ocrPageJson = JsonSerializer.Serialize(ocrPage, Constants.DefaultJsonSerializerOptions);
            File.WriteAllText(ocrFilePath, ocrPageJson);
            writeMeta(ocrPage);
        }

        bool getIsPageExcluded(int pageNumber)
        {
            return (bookInfo.ExcludedPages.Any(x => pageNumber >= x.First && pageNumber <= x.Last));
        }

        void writeMeta(OcrPage page)
        {
            int wordCount = page.Words.Count;
            var pageMeta = new OcrPageMeta { PageNumber = pageNumber, NumberOfWords = wordCount };
            string metaJson = JsonSerializer.Serialize(pageMeta, Constants.DefaultJsonSerializerOptions);
            File.WriteAllText(ocrMetaFilePath, metaJson);
        }
    }

    private static TesseractEngine CreateEngine(string editionCode, bool multiColumn)
    {
        string directoryPath = @"./tessdata";
        var engine = new TesseractEngine(directoryPath, "eng", EngineMode.LstmOnly);
        if (multiColumn)
        {
            engine.DefaultPageSegMode = PageSegMode.Auto;
        }
        else
        {
            engine.DefaultPageSegMode = PageSegMode.SingleColumn;
        }
        engine.SetVariable("tessedit_char_whitelist", AllowedScanChars);

        // Disable dictionary-based correction (prevents unwanted accents)
        engine.SetVariable("load_system_dawg", "0");
        engine.SetVariable("load_freq_dawg", "0");
        engine.SetVariable("OMP_THREAD_LIMIT", "1");
        engine.SetVariable("preserve_interword_spaces", "1");
        engine.SetVariable("classify_enable_learning", "0");
        return engine;
    }

    public static int EstimateWordWidth(int height, string text)
    {
        double width = 0;
        double mFactor = LetterHeightToWidthFactors['m'];
        foreach (char c in text)
        {
            if (!LetterHeightToWidthFactors.TryGetValue(c, out double widthFactor))
                widthFactor = mFactor;
            width += (height * widthFactor);
        }
        return (int)Math.Floor(width);
    }


    // Helper: Gets the bounding box for the current symbol from the OCR iterator.
    private static OcrRect GetSymbolBoundingBox(ResultIterator iterator, char c, int upscaleFactor)
    {
        if (iterator.TryGetBoundingBox(PageIteratorLevel.Symbol, out var tRect))
        {
            OcrRect result = ConvertRect(tRect);
            if (LetterHeightToWidthFactors.TryGetValue(c, out double expectedWidthRatio))
            {
                float encounteredWidthRatio = tRect.Width / (float)tRect.Height;
                if (encounteredWidthRatio > expectedWidthRatio)
                    result = result with { Width = (int)Math.Round(result.Height * expectedWidthRatio, MidpointRounding.AwayFromZero) };
            }
            return result with { X = result.X / upscaleFactor, Y = result.Y / upscaleFactor, Width = result.Width / upscaleFactor, Height = result.Height / upscaleFactor };
        }
        return OcrRect.Empty;
    }

    // Helper: Converts a Tesseract.Rect to our OcrRect.
    private static OcrRect ConvertRect(Rect tRect)
    {
        return new OcrRect { X = tRect.X1, Y = tRect.Y1, Width = tRect.Width, Height = tRect.Height };
    }

    private OcrWord[] ProcessImageInternal(OcrBookInfo bookInfo, MagickImage sourceImage, int upscaleFactor = DefaultUpscaleFactor)
    {
        if (!IsProcessing) return [];

        var words = new List<OcrWord>();
        using var image = sourceImage.Clone();

        image.ColorType = ColorType.Grayscale;
        if (!IsProcessing) return [];

        image.AutoLevel();
        if (!IsProcessing) return [];

        image.Normalize();
        if (!IsProcessing) return [];

        image.UnsharpMask(radius: 1.0, sigma: 0.5, amount: 1.0, threshold: 0.05);
        if (!IsProcessing) return [];

        image.Despeckle();
        if (!IsProcessing) return [];

        image.Despeckle();
        if (!IsProcessing) return [];

        if (upscaleFactor != 1)
        {
            image.FilterType = FilterType.Box;
            if (!IsProcessing) return [];

            image.Resize(sourceImage.Width * (uint)upscaleFactor, sourceImage.Height * (uint)upscaleFactor);
            if (!IsProcessing) return [];
        }

        // Convert the MagickImage to a Pix object (here we write to PNG in memory)
        byte[] imageBytes = image.ToByteArray(MagickFormat.Png);
        if (!IsProcessing) return [];

        using var pix = Pix.LoadFromMemory(imageBytes);
        if (!IsProcessing) return [];
        // Initialize Tesseract OCR engine.
        // Ensure that the tessdata directory is available at the given path.
        using TesseractEngine pageEngine = CreateEngine(bookInfo.Code, bookInfo.MultiColumn);
        if (!IsProcessing) return [];

        using var page = pageEngine.Process(pix);
        if (!IsProcessing) return [];

        // Use the ResultIterator to walk through recognized symbols.
        // We iterate at the symbol level so we can separate punctuation.
        using ResultIterator iterator = page.GetIterator();
        iterator.Begin();
        if (!IsProcessing) return [];

        var characterElements = new List<OcrElement>();

        do
        {
            if (iterator.IsAtBeginningOf(PageIteratorLevel.Word))
            {
                addCurrentElements();
            }

            // Get the text of the current symbol.
            string? symbolText = iterator.GetText(PageIteratorLevel.Symbol);

            // We assume symbolText is one character.
            char c = symbolText[0];

            if (!AllowedChars.Contains(c))
            {
                addCurrentElements();
            }
            else
            {
                // Get the bounding box of the current symbol.
                OcrRect symbolBox = GetSymbolBoundingBox(iterator, c, upscaleFactor);
                OcrElement currentCharElement = new OcrElement { Text = c.ToString(), Bounds = symbolBox };

                characterElements.Add(currentCharElement);
            }
        } while (IsProcessing && iterator.Next(PageIteratorLevel.Symbol));

        // In case there are any elements left
        addCurrentElements();

        if (!IsProcessing) return [];
        return words.ToArray();

        void addCurrentElements()
        {
            if (characterElements.Count == 0) return;

            if (characterElements.Count > 1)
            {
                int dashIndex = characterElements.FindIndex(x => x.Text == "-");
                if (dashIndex > -1)
                {
                    var originalCharacterElements = characterElements;
                    characterElements = originalCharacterElements[..dashIndex];
                    addCurrentElements();
                    characterElements = [originalCharacterElements[dashIndex]];
                    addCurrentElements();
                    characterElements = originalCharacterElements[(dashIndex + 1)..];
                    addCurrentElements();
                    return;
                }
            }

            //string fulltext = string.Join("", characterElements.Select(x => x.Text)).ToUpper();
            //if (fulltext.Contains("RING"))
            //    System.Diagnostics.Debugger.Break();

            if (firstCharIsSuperScript())
                characterElements.RemoveAt(0);

            normalizeChars();
            if (characterElements.Count == 0) return;

            OcrElement firstElement = characterElements[0];
            OcrRect bounds = firstElement.Bounds;
            string text = firstElement.Text;
            foreach (OcrElement element in characterElements.Skip(1))
            {
                bounds = bounds.Union(element.Bounds);
                text += element.Text;
            }
            var currentElement = new OcrElement { Bounds = bounds, Text = text };
            var word = new OcrWord { Elements = [currentElement] };
            words.Add(word);
            characterElements.Clear();
        }

        bool firstCharIsSuperScript()
        {
            if (characterElements.Count == 1) return false;
            int firstCharBottom = characterElements[0].Bounds.GetBottom();
            double averageMidPoint = characterElements.Skip(1).Average(x => x.Bounds.GetCenter().Y);
            return firstCharBottom < averageMidPoint;
        }

        void normalizeChars()
        {
            if (characterElements.Count == 1)
            {
                characterElements = characterElements.Select(x => x with {
                    Text = x.Text[0] switch {
                        '1' => "I",
                        '0' => "O",
                        _ => x.Text
                    }
                }).ToList();
                char c = characterElements[0].Text.ToUpperInvariant()[0];
                if (c != 'I' && c != 'A' && c != 'O' && c != '-')
                    characterElements.Clear();
            }
            if (characterElements.All(x => char.IsDigit(x.Text[0])))
                characterElements.Clear();
        }

    }
}

