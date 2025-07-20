using ConvertImagesToText;
using DocumentsModel;
using DocumentsModel.Helpers;
using ImageMagick;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.Immutable;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Extensions;
using WordsAnalysis.Services;

namespace WordsAnalysis.Components;

public partial class EditWordDialog : IAsyncDisposable
{
    public record EditWordDialogContent(EditionState Edition, WordReference WordReference, int PageWidth, int PageHeight, bool IsAdd);
    public record EditWordDialogResult(OcrWord? Word, bool After);

    [Parameter]
    public EditWordDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private bool AddWordAfter = true;
    private bool ApplyThreshold;
    private KeyValuePair<BenefitOfDoubt, string> BenefitOfDoubtSelectedOption;
    private string? BenefitOfDoubtText;
    private EditForm EditForm = null!;
    private bool HasEstimatedSize;
    private int LineHeight;
    private int LineHeightAdjustment;
    private bool LineHeightLarger;
    private string? Notes = "";
    private OcrRect OriginalBounds = OcrRect.Empty;
    private MagickImage PageImage = null!;
    private string? PageImageData;
    private PageState PageState = null!;
    private bool ShowDashes;
    private bool ShowHighContrast;
    private bool ShowSurroundingText;
    private int ThresholdLower;
    private int ThresholdUpper;
    private TextData[] Texts = [];
    private OcrWord Word = null!;
    private string? WordImageData;

    private static readonly IEnumerable<BenefitOfDoubt> BenefitOfDoubtOptions;

    static EditWordDialog()
    {
        BenefitOfDoubtOptions = Enum.GetValues<BenefitOfDoubt>();
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        PageImage?.Dispose();
        return ValueTask.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ReadAppSettings();
        ResetLineHeightAdjustment();

        PageState = Content.Edition.LoadedPages[Content.WordReference.PageNumber];
        Word = Content.WordReference.GetWord(Content.Edition)!;
        if (Content.IsAdd)
        {
            OcrRect bounds = Word.Elements.Last().Bounds;
            int xOffset = bounds.Width + OcrProcessor.EstimateWordSize(LineHeight, bounds.Height, "i").Width;
            Texts = [new TextData("", bounds.Offset(xOffset, 0), false)];
            ShowDashes = false;
            Word = new OcrWord { Elements = [Word.Elements[0] with { Text = "" }] };
        }
        else
        {
            Texts = Word.Elements.Select(x => new TextData(x.Text, x.Bounds, x.IsOnNextPage)).ToArray();
            ShowDashes = Word.ShowDashes;
        }

        OriginalBounds = Word.Elements[0].Bounds;
        Notes = Word.Notes;
        BenefitOfDoubtSelectedOption = BenefitOfDoubtExtensions.GetOptions().First(x => x.Key == Word.BenefitOfDoubt);
        BenefitOfDoubtText = Word.BenefitOfDoubtText;
        LoadImageData();
    }

    private async Task CancelAsync()
    {
        var result = new EditWordDialogResult(null, false);
        await Dialog.CancelAsync(result);
    }

    private async Task ConfirmAsync()
    {
        if (!EditForm.EditContext!.Validate()) return;
        WriteAppSettings();

        OcrWord? newWord = CreateWord();
        if (newWord != null)
            await Clipboard.SetTextAsync(newWord.GetCombinedText());

        var result = new EditWordDialogResult(newWord, AddWordAfter);
        await Dialog.CloseAsync(result);
    }

    private OcrWord CreateWord()
    {
        ImmutableList<OcrElement> newElements = Texts.Select(x => new OcrElement { Text = x.Text, Bounds = x.Bounds, IsOnNextPage = x.IsOnNextPage }).ToImmutableList();
        OcrWord result = Word with { 
            Elements = newElements,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
            ShowDashes = ShowDashes,
            BenefitOfDoubt = BenefitOfDoubtSelectedOption.Key,
            BenefitOfDoubtText = BenefitOfDoubtText };
        if (result.BenefitOfDoubt == BenefitOfDoubt.None)
        {
            result = result with { BenefitOfDoubtText = null };
        }
        return result;
    }

    private void DropFirstLetter(int elementIndex)
    {
        TextData item = Texts[elementIndex];
        string text = item.Text;
        if (text.Length < 2) return;

        char firstLetter = item.Text[0];
        int estimatedWidth = (int)(OcrProcessor.EstimateWordSize(LineHeight, item.Bounds.Height, firstLetter.ToString()).Height * 0.6d);

        item.Text = text[1..];
        item.Bounds = item.Bounds with {
            X = item.Bounds.X + estimatedWidth,
            Width = Math.Max(1, item.Bounds.Width - estimatedWidth)
        };
        UpdateImageData();
    }

    private void EstimateWordSize(int elementIndex)
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
        System.Drawing.Size estimatedSize = OcrProcessor.EstimateWordSize(LineHeight + (LineHeightAdjustment * lineHeightAdjustmentFactor), item.Bounds.Height, text);
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

        UpdateImageData();

        HasEstimatedSize = true;
    }

    private string GetActionName()
    {
        if (Content.IsAdd)
            return "Add";
        else
            return "Edit";
    }

    private void LoadImageData()
    {
        string imageFilePath = FilePathHelper.GetScansDeskewedImageFilePath(AppLayer.Constants.Data.SourcesDirectoryPath, Content.WordReference.BookInfo, Content.WordReference.PageNumber);
        PageImage = new MagickImage(imageFilePath);
        PageImageData = PageImage.ToEmbeddedHtmlImage();
        UpdateImageData();
    }

    private void Move(MouseEventArgs e, int elementIndex, int xFactor, int yFactor)
    {
        ResetLineHeightAdjustment();
        bool wasAfter = elementIndex != 0 || isAfter();
        int changeSize = e.CtrlKey ? (LineHeight / 2) : 1;
        int xAdjustment = xFactor * changeSize;
        int yAdjustment = yFactor * changeSize;
        OcrRect bounds = Texts[elementIndex].Bounds;
        if (e.AltKey && yAdjustment == 0 && e.ShiftKey)
        {
            if (xAdjustment < 0)
            {
                var page = Content.Edition.LoadedPages[Content.WordReference.PageNumber].Page;
                var wordsBefore = page.Words.Where((x, index) => x != null && index < Content.WordReference.WordIndex);
                var leftPositions = wordsBefore.SelectMany(x => x!.Elements).Select(x => x.Bounds.X);
                int leftMost = leftPositions.Any() ? leftPositions.Min() : 0;
                Texts[elementIndex].Bounds = Texts[elementIndex].Bounds = (bounds.Offset(0, bounds.Height) with { X = leftMost });
            }
        }
        else
        {
            if (e.ShiftKey)
            {
                if (e.AltKey)
                    bounds = bounds with { Height = Math.Max(1, bounds.Height - yAdjustment) };
                Texts[elementIndex].Bounds = bounds.Offset(xAdjustment, yAdjustment);
            }
            else
                Texts[elementIndex].Bounds = bounds with {
                    Width = Math.Max(1, bounds.Width + xAdjustment),
                    Height = Math.Max(1, bounds.Height + yAdjustment)
                };
        }
        bool newIsAfter = elementIndex != 0 || isAfter();
        if (wasAfter != newIsAfter)
            AddWordAfter = newIsAfter;
        UpdateImageData();
        StateHasChanged();

        bool isAfter()
        {
            OcrRect bounds = Texts[0].Bounds;
            int middle = bounds.Y + (bounds.Height / 2);
            if (middle > OriginalBounds.GetBottom()) return true;
            if (middle < OriginalBounds.Y) return false;
            if (bounds.X < OriginalBounds.X) return false;
            return true;
        }
    }

    private void MoveDown(MouseEventArgs e, int elementIndex)
    {
        Move(e, elementIndex, 0, 1);
    }

    private void MoveLeft(MouseEventArgs e, int elementIndex)
    {
        Move(e, elementIndex, -1, 0);
    }

    private void MoveRight(MouseEventArgs e, int elementIndex)
    {
        Move(e, elementIndex, 1, 0);
    }

    private void MoveUp(MouseEventArgs e, int elementIndex)
    {
        Move(e, elementIndex, 0, -1);
    }

    private void ResetLineHeightAdjustment()
    {
        LineHeightAdjustment = 0;
        LineHeightLarger = false;
        HasEstimatedSize = false;
    }


    private void ThresholdLowerChanged()
    {
        if (ThresholdLower >= ThresholdUpper)
            ThresholdUpper = ThresholdLower + 1;
        UpdateImageData();
    }

    private void ThresholdUpperChanged()
    {
        if (ThresholdUpper <= ThresholdLower)
            ThresholdLower = ThresholdUpper - 1;
        UpdateImageData();
    }

    private void UpdateImageData()
    {
        OcrWord tempWord = CreateWord();
        PageState.ImageOptions? imageOptions =
            !ShowHighContrast
            ? null
            : new PageState.ImageOptions {
                ApplyThreshold = ApplyThreshold,
                ShowHighContrast = ShowHighContrast,
                ThresholdLower = ThresholdLower,
                ThresholdUpper = ThresholdUpper
            };
        using MagickImage lineImage = PageState.GetWordImage(PageImage, tempWord, ShowSurroundingText, imageOptions);
        WordImageData = lineImage.ToEmbeddedHtmlImage();
    }

    private void ReadAppSettings()
    {
        // Edition
        LineHeight = AppPreferences.Editions.GetLineHeight(Content.Edition.BookInfo);
        // Image
        ApplyThreshold = AppPreferences.EditWordDialog.ApplyThreshold;
        ShowHighContrast = AppPreferences.EditWordDialog.ShowHighContrast;
        ShowSurroundingText = AppPreferences.EditWordDialog.ShowSurroundingText;
        ThresholdLower = AppPreferences.EditWordDialog.ThresholdLower;
        ThresholdUpper = AppPreferences.EditWordDialog.ThresholdUpper;
    }

    private void WriteAppSettings()
    {
        // Edition
        AppPreferences.Editions.SetLineHeight(Content.Edition.BookInfo, LineHeight);
        // Image
        AppPreferences.EditWordDialog.ApplyThreshold = ApplyThreshold;
        AppPreferences.EditWordDialog.ShowHighContrast = ShowHighContrast;
        AppPreferences.EditWordDialog.ShowSurroundingText = ShowSurroundingText;
        AppPreferences.EditWordDialog.ThresholdLower = ThresholdLower;
        AppPreferences.EditWordDialog.ThresholdUpper = ThresholdUpper;
    }

    private class TextData
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
