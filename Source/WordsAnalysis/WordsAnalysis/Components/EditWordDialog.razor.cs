using ConvertImagesToText;
using DocumentsModel;
using DocumentsModel.Helpers;
using ImageMagick;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Extensions;

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
    private EditForm EditForm = null!;
    private bool HasEstimatedSize;
    private int LineHeight;
    private int LineHeightAdjustment;
    private string LineHeightPreferenceKey = "";
    private string? Notes;
    private OcrRect OriginalBounds = OcrRect.Empty;
    private MagickImage PageImage = null!;
    private string? PageImageData;
    private PageState PageState = null!;
    private bool ShowDashes;
    private bool _ShowSurroundingText = true;
    public bool ShowSurroundingText { get => _ShowSurroundingText; set => _ShowSurroundingText = value; }
    private RequiredText[] Texts = [];
    private OcrWord Word = null!;
    private string? WordImageData;

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        PageImage?.Dispose();
        return ValueTask.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        LineHeightPreferenceKey = $"{Content.Edition.BookInfo.Code}-LineHeight";
        LineHeight = Preferences.Get(LineHeightPreferenceKey, 12);
        ResetLineHeightAdjustment();

        PageState = Content.Edition.LoadedPages[Content.WordReference.PageNumber];
        Word = Content.WordReference.GetWord(Content.Edition)!;
        OriginalBounds = Word.Elements[0].Bounds;
        if (Content.IsAdd)
        {
            OcrRect bounds = Word.Elements.Last().Bounds;
            int xOffset = bounds.Width + OcrProcessor.EstimateWordSize(LineHeight, bounds.Height, "i").Width;
            Texts = [new RequiredText("", bounds.Offset(xOffset, 0), false)];
            ShowDashes = false;
        }
        else
        {
            Texts = Word.Elements.Select(x => new RequiredText(x.Text, x.Bounds, x.IsOnNextPage)).ToArray();
            ShowDashes = Word.ShowDashes;
        }
        Notes = Word.Notes;
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

        OcrWord? newWord = CreateWord();

        if (newWord != null)
            await Clipboard.SetTextAsync(newWord.GetCombinedText());

        Preferences.Set(LineHeightPreferenceKey, LineHeight);

        var result = new EditWordDialogResult(newWord, AddWordAfter);
        await Dialog.CloseAsync(result);
    }

    private OcrWord CreateWord()
    {
        string? notes = String.IsNullOrWhiteSpace(Notes) ? null : Notes;
        ImmutableList<OcrElement> newElements = Texts.Select(x => new OcrElement { Text = x.Text, Bounds = x.Bounds, IsOnNextPage = x.IsOnNextPage }).ToImmutableList();
        OcrWord result = Word with { Elements = newElements, ShowDashes = ShowDashes, Notes = notes };
        return result;
    }

    private void DropFirstLetter(int elementIndex)
    {
        RequiredText item = Texts[elementIndex];
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
        RequiredText item = Texts[elementIndex];
        string text = item.Text;

        if (HasEstimatedSize)
        {
            LineHeightAdjustment-=2;
            if (Math.Abs(LineHeightAdjustment) > LineHeight / 2)
            {
                LineHeightAdjustment = LineHeight / 2;
            }
        }
        System.Drawing.Size estimatedSize = OcrProcessor.EstimateWordSize(LineHeight + LineHeightAdjustment, item.Bounds.Height, text);
        int yAdjustment = (item.Bounds.Height - estimatedSize.Height) / 2;
        Texts[elementIndex].Bounds = item.Bounds with { Y = item.Bounds.Y + yAdjustment, Width = estimatedSize.Width, Height = estimatedSize.Height };
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
        int xSize = e.CtrlKey ? LineHeight : 1;
        int ySize = e.CtrlKey ? (LineHeight / 2) : 1;
        int xAdjustment = xFactor * xSize;
        int yAdjustment = yFactor * ySize;
        OcrRect bounds = Texts[elementIndex].Bounds;
        if (e.AltKey && yAdjustment == 0 && e.ShiftKey)
        {
            if (xAdjustment < 0)
                Texts[elementIndex].Bounds = bounds.Offset(-bounds.X, bounds.Height);
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
        HasEstimatedSize = false;
    }

    private void ShowSurroundingTextChanged()
    {
        UpdateImageData();
    }

    private void UpdateImageData()
    {
        OcrWord tempWord = CreateWord();
        using MagickImage lineImage = PageState.GetWordImage(PageImage, tempWord, ShowSurroundingText);
        WordImageData = lineImage.ToEmbeddedHtmlImage();
    }

    private class RequiredText
    {
        public OcrRect Bounds { get; set; }

        public bool IsOnNextPage { get; set; }

        [Required]
        public string Text { get; set; } = null!;

        public RequiredText(string text, OcrRect bounds, bool isOnNextPage)
        {
            Text = text;
            Bounds = bounds;
        }
    }
}
