using ConvertImagesToText;
using DocumentsModel;
using DocumentsModel.Helpers;
using System.Collections.Immutable;
using WordsAnalysis.AppLayer.Services;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public class EditWordDialogViewModel
{
    public bool AddWordAfter = true;
    public bool ApplyThreshold;
    public KeyValuePair<BenefitOfDoubt, string> BenefitOfDoubtSelectedOption;
    public string? BenefitOfDoubtText;
    public bool Corrected;
    public bool Correction;
    public bool HasEstimatedSize;
    public bool HasSampleImages;
    public bool Inserted;
    public int LineHeight;
    public int LineHeightAdjustment;
    public bool LineHeightLarger;
    public string? Notes = "";
    public OcrRect OriginalBounds = OcrRect.Empty;
    public bool ShowDashes;
    public bool ShowHighContrast;
    public int ThresholdLower;
    public int ThresholdUpper;
    public TextData[] Texts = [];

    public static readonly IEnumerable<BenefitOfDoubt> BenefitOfDoubtOptions = Enum.GetValues<BenefitOfDoubt>();

    private OcrWord Word = null!;

    public void Initialize(EditWordDialogContent content, bool hasSampleImages)
    {
        HasSampleImages = hasSampleImages;
        ResetLineHeightAdjustment();

        Word = content.WordReference.GetWord(content.Edition)!;
        if (content.IsAdd)
        {
            OcrElement lastElementOnSamePage = Word.LastElementOnSamePage();
            OcrRect bounds = lastElementOnSamePage.Bounds;
            int xOffset = bounds.Width + OcrProcessor.EstimateWordSize(LineHeight, "i").Width;
            Texts = [new TextData("", bounds.Offset(xOffset, 0) with { Width = LineHeight }, false)];
            ShowDashes = false;
            Word = new OcrWord { Elements = [lastElementOnSamePage with { Text = "" }] };
        }
        else
        {
            Texts = Word.Elements.Select(x => new TextData(x.Text, x.Bounds, x.IsOnNextPage)).ToArray();
            ShowDashes = Word.ShowDashes;
        }

        OriginalBounds = Word.Elements[0].Bounds;
        Notes = Word.Notes;
        Corrected = Word.Corrected;
        Correction = Word.Correction;
        Inserted = Word.Inserted;
        BenefitOfDoubtSelectedOption = BenefitOfDoubtExtensions.GetOptions().First(x => x.Key == Word.BenefitOfDoubt);
        BenefitOfDoubtText = Word.BenefitOfDoubtText;
    }

    public void ConvertAmpersand()
    {
        BenefitOfDoubtSelectedOption = BenefitOfDoubtExtensions.GetOptions().First(x => x.Key == BenefitOfDoubt.PrinterError);
        BenefitOfDoubtText = BenefitOfDoubtText == "and" ? "And" : "and";
        Texts[0].Text = "M";
        EstimateWordSize(0);
        Texts[0].Text = "&";
    }

    public OcrWord CreateWord()
    {
        ImmutableList<OcrElement> newElements = Texts.Select(x => new OcrElement { Text = x.Text, Bounds = x.Bounds, IsOnNextPage = x.IsOnNextPage }).ToImmutableList();
        OcrWord result = Word with {
            Elements = newElements,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
            Corrected = Corrected,
            Correction = Correction,
            Inserted = Inserted,
            ShowDashes = ShowDashes,
            BenefitOfDoubt = BenefitOfDoubtSelectedOption.Key,
            BenefitOfDoubtText = BenefitOfDoubtText
        };
        if (result.BenefitOfDoubt == BenefitOfDoubt.None)
        {
            result = result with { BenefitOfDoubtText = null };
        }
        return result;
    }

    public void DropFirstLetter(int elementIndex)
    {
        TextData item = Texts[elementIndex];
        string text = item.Text;
        if (text.Length < 2) return;

        char firstLetter = item.Text[0];
        int estimatedWidth = (int)(OcrProcessor.EstimateWordSize(LineHeight, firstLetter.ToString()).Height * 0.6d);

        item.Text = text[1..];
        item.Bounds = item.Bounds with {
            X = item.Bounds.X + estimatedWidth,
            Width = Math.Max(1, item.Bounds.Width - estimatedWidth)
        };
    }

    public void EstimateWordSize(int elementIndex)
    {
        TextData item = Texts[elementIndex];
        string text = item.Text;

        if (HasEstimatedSize)
        {
            if (LineHeightLarger)
            {
                LineHeightAdjustment -= 2;
                LineHeightLarger = false;
            }
            else
            {
                LineHeightLarger = true;
            }

            if (Math.Abs(LineHeightAdjustment) > LineHeight / 2)
            {
                LineHeightAdjustment = LineHeight / 2;
            }
        }
        int lineHeightAdjustmentFactor =
            LineHeightLarger ? 1 : -1;
        System.Drawing.Size estimatedSize = OcrProcessor.EstimateWordSize(LineHeight + (LineHeightAdjustment * lineHeightAdjustmentFactor), text);
        int yAdjustment = (item.Bounds.Height - estimatedSize.Height) / 2;
        Texts[elementIndex].Bounds = item.Bounds with { Y = item.Bounds.Y + yAdjustment, Width = estimatedSize.Width, Height = estimatedSize.Height };

        if (Texts.Length == 1)
        {
            double factor = Texts[0].Text switch {
                "I" => 2,
                "A" => 2,
                "a" => 2,
                _ => 1
            };
            Texts[0].Bounds = Texts[0].Bounds with { Width = (int)(Texts[0].Bounds.Width * factor) };
        }

        HasEstimatedSize = true;
    }

    public CalculateMoveResult CalculateMove(bool ctrlKey, bool shiftKey, bool altKey, int elementIndex, int xFactor, int yFactor, OcrPage page, int wordIndex)
    {
        ResetLineHeightAdjustment();
        bool wasAfter = elementIndex != 0 || IsAfter();
        int changeSize = ctrlKey ? 1 : (LineHeight / 4);
        int xAdjustment = xFactor * changeSize;
        int yAdjustment = yFactor * changeSize;
        OcrRect bounds = Texts[elementIndex].Bounds;
        bool shouldCenter = false;
        if (altKey && yAdjustment == 0 && shiftKey)
        {
            if (xAdjustment < 0)
            {
                var wordsBefore = page.Words.Where((x, index) => x != null && index < wordIndex);
                var leftPositions = wordsBefore.SelectMany(x => x!.Elements).Select(x => x.Bounds.X);
                int leftMost = Math.Max(0, leftPositions.Any() ? leftPositions.Min() : 0);
                Texts[elementIndex].Bounds = bounds.Offset(0, bounds.Height) with { X = leftMost };
                shouldCenter = true;
            }
        }
        else
        {
            if (shiftKey)
            {
                if (altKey)
                    bounds = bounds with { Height = Math.Max(1, bounds.Height - yAdjustment) };
                Texts[elementIndex].Bounds = bounds.Offset(xAdjustment, yAdjustment);
            }
            else
                Texts[elementIndex].Bounds = bounds with {
                    Width = Math.Max(0, bounds.Width + xAdjustment),
                    Height = Math.Max(0, bounds.Height + yAdjustment)
                };
        }
        bool newIsAfter = elementIndex != 0 || IsAfter();
        bool addWordAfterChanged = wasAfter != newIsAfter;
        if (addWordAfterChanged)
            AddWordAfter = newIsAfter;

        return new CalculateMoveResult(shouldCenter, addWordAfterChanged, Texts[elementIndex].Bounds);
    }

    public string GetActionName()
    {
        if (Content.IsAdd)
            return "Add";
        else
            return "Edit";
    }

    public PageState.ImageOptions? GetImageOptions()
    {
        return !ShowHighContrast
            ? null
            : new PageState.ImageOptions {
                ApplyThreshold = ApplyThreshold,
                ShowHighContrast = ShowHighContrast,
                ThresholdLower = ThresholdLower,
                ThresholdUpper = ThresholdUpper
            };
    }

    public void ReadAppSettings(IAppPreferences appPreferences, EditWordDialogContent content)
    {
        // Edition
        LineHeight = appPreferences.Editions.GetLineHeight(content.Edition.BookInfo);
        // Image
        ApplyThreshold = appPreferences.EditWordDialog.ApplyThreshold;
        ShowHighContrast = appPreferences.EditWordDialog.ShowHighContrast;
        ThresholdLower = appPreferences.EditWordDialog.ThresholdLower;
        ThresholdUpper = appPreferences.EditWordDialog.ThresholdUpper;
    }

    public void WriteAppSettings(IAppPreferences appPreferences, EditWordDialogContent content)
    {
        // Edition
        appPreferences.Editions.SetLineHeight(content.Edition.BookInfo, LineHeight);
        // Image
        appPreferences.EditWordDialog.ApplyThreshold = ApplyThreshold;
        appPreferences.EditWordDialog.ShowHighContrast = ShowHighContrast;
        appPreferences.EditWordDialog.ThresholdLower = ThresholdLower;
        appPreferences.EditWordDialog.ThresholdUpper = ThresholdUpper;
    }

    public void ResetLineHeightAdjustment()
    {
        LineHeightAdjustment = 0;
        LineHeightLarger = false;
        HasEstimatedSize = false;
    }

    public void ThresholdLowerChanged()
    {
        if (ThresholdLower >= ThresholdUpper)
            ThresholdUpper = ThresholdLower + 1;
    }

    public void ThresholdUpperChanged()
    {
        if (ThresholdUpper <= ThresholdLower)
            ThresholdLower = ThresholdUpper - 1;
    }

    private EditWordDialogContent Content = null!;

    public void SetContent(EditWordDialogContent content)
    {
        Content = content;
    }

    private bool IsAfter()
    {
        OcrRect bounds = Texts[0].Bounds;
        int middle = bounds.Y + (bounds.Height / 2);
        if (middle > OriginalBounds.GetBottom()) return true;
        if (middle < OriginalBounds.Y) return false;
        if (bounds.X < OriginalBounds.X) return false;
        return true;
    }

    public class TextData
    {
        public OcrRect Bounds { get; set; }

        public bool IsOnNextPage { get; set; }

        public string Text { get; set; } = null!;

        public TextData(string text, OcrRect bounds, bool isOnNextPage)
        {
            Text = text;
            Bounds = bounds;
            IsOnNextPage = isOnNextPage;
        }
    }
}

public record CalculateMoveResult(bool ShouldCenter, bool AddWordAfterChanged, OcrRect NewBounds);
