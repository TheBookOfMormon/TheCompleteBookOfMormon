using ConvertImagesToText;
using DocumentsModel;
using DocumentsModel.Helpers;
using ImageMagick;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Extensions;

namespace WordsAnalysis.Components;

public partial class RescanAreaDialog : IAsyncDisposable
{
    public record RescanAreaDialogContent(EditionState Edition, WordReference WordReference);
    public record EditWordDialogResult(IEnumerable<OcrWord> Words);

    [Parameter]
    public RescanAreaDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private OcrRect Bounds = OcrRect.Empty;
    private MagickImage PageImage = null!;
    private int PageWidth;
    private PageState PageState = null!;
    private string? ScannedText;
    private string? WordImageData;
    private OcrWord[] Words = [];

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        PageImage?.Dispose();
        return ValueTask.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Bounds = Content.WordReference.GetWord(Content.Edition)?.Elements[0].Bounds ?? OcrRect.Empty;
        PageState = Content.Edition.LoadedPages[Content.WordReference.PageNumber];
        LoadImageData();
        if (Bounds == OcrRect.Empty) await CancelAsync();
    }

    private async Task CancelAsync()
    {
        var result = new EditWordDialogResult([]);
        await Dialog.CancelAsync(result);
    }

    private async Task ConfirmAsync()
    {
        var result = new EditWordDialogResult(Words);
        await Dialog.CloseAsync(result);
    }

    private void LoadImageData()
    {
        string imageFilePath = FilePathHelper.GetScansDeskewedImageFilePath(AppLayer.Constants.Data.SourcesDirectoryPath, Content.WordReference.BookInfo, Content.WordReference.PageNumber);
        PageImage = new MagickImage(imageFilePath);
        PageWidth = (int)PageImage.Width;
        UpdateImageData();
    }

    private void Move(MouseEventArgs e, int xFactor, int yFactor)
    {
        int size = e.CtrlKey ? (PageWidth / 75) : 1;
        int xAdjustment = xFactor * size;
        int yAdjustment = yFactor * size;
        if (e.AltKey && yAdjustment == 0 && e.ShiftKey)
        {
            if (xAdjustment < 0)
            {
                Bounds = Bounds.Offset(-Bounds.X, Bounds.Height);
            }
        }
        else
        {
            if (e.ShiftKey)
                Bounds = Bounds.Offset(xAdjustment, yAdjustment);
            else
                Bounds = Bounds with {
                    Width = Math.Max(1, Bounds.Width + xAdjustment),
                    Height = Math.Max(1, Bounds.Height + yAdjustment)
                };
        }
        UpdateImageData();
        StateHasChanged();
    }

    private void MoveDown(MouseEventArgs e)
    {
        Move(e, 0, 1);
    }

    private void MoveLeft(MouseEventArgs e)
    {
        Move(e, -1, 0);
    }

    private void MoveRight(MouseEventArgs e)
    {
        Move(e, 1, 0);
    }

    private void MoveUp(MouseEventArgs e)
    {
        Move(e, 0, -1);
    }

    private void Rescan()
    {
        using var ocrProcessor = new OcrProcessor(AppLayer.Constants.Data.SourcesDirectoryPath);
        using var croppedImage = PageImage.CloneArea(Bounds);
        OcrWord[] scannedWords = ocrProcessor.ProcessImage(Content.WordReference.BookInfo, croppedImage, false, 16);
        Words = scannedWords.Select(w => w with { Elements = w.Elements.Select(e => e with { Bounds = e.Bounds.Offset(Bounds.X, Bounds.Y) }).ToImmutableList() }).ToArray();
        ScannedText = string.Join(' ', Words.SelectMany(x => x.Elements.Select(x => x.Text)));
    }

    private void UpdateImageData()
    {
        OcrElement tempElement = new OcrElement { Bounds = Bounds, Text = "" };
        OcrWord tempWord = new OcrWord {
            Elements = [tempElement]
        };
        using MagickImage lineImage = PageState.GetWordImage(PageImage, tempWord);
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
 