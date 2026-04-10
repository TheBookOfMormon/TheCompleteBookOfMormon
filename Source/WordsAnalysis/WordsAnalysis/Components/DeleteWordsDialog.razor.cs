using DocumentsModel;
using DocumentsModel.Helpers;
using ImageMagick;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Extensions;

namespace WordsAnalysis.Components;

public partial class DeleteWordsDialog
{

    [Parameter]
    public DeleteWordsDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private int DeleteCount;
    private int DeletePosition;
    private OcrElement FirstElement = null!;
    private WordReference FirstWordReference = null!;
    private string PageImageData = "";
    private int PageHeight;
    private int PageWidth;
    private DeleteWordsPosition Position = DeleteWordsPosition.Sequential;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        string imageFilePath = FilePathHelper.GetScansDeskewedImageFilePath(AppLayer.Constants.Data.SourcesDirectoryPath, Content.EditionState.BookInfo, Content.Words.First().Key.PageNumber);
        using var pageImage = new MagickImage(imageFilePath);
        PageHeight = (int)pageImage.Height;
        PageWidth = (int)pageImage.Width;
        PageImageData = pageImage.ToEmbeddedHtmlImage()!;

        FirstWordReference = Content.Words.First().Key;
        var firstWord = Content.EditionState.LoadedPages[FirstWordReference.PageNumber].Page.Words[FirstWordReference.WordIndex]!;
        FirstElement = firstWord.Elements.Last();

        // About index
        int? aboutIndex = Content.Words.Select((word, index) => new { Index = index, Text = word.Value }).FirstOrDefault(x => string.Equals("about", x.Text, StringComparison.InvariantCultureIgnoreCase) && x.Text![0] == 'A')?.Index;
        if (aboutIndex != null)
        {
            if (aboutIndex < Content.Words.Length - 1)
            {
                if (Content.Words[aboutIndex.Value + 1].Value == "-")
                    aboutIndex++;
            }
        }

        // Next column index
        int? nextColumnIndex = null;
        // Try to find start of next column
        for (int index = 2; index < Content.Words.Length; index++)
        {
            OcrWord? candidateWord = Content.Words[index].Key.GetWord(Content.EditionState);
            if (candidateWord is null) continue;

            OcrElement candidateElement = candidateWord.Elements.Last();
            if (candidateElement.Bounds.Y + candidateElement.Bounds.Height < FirstElement.Bounds.Y)
            {
                nextColumnIndex = index;
                break;
            }
        }

        // Get the lowest non-null value
        int? candidateIndex = (aboutIndex, nextColumnIndex) switch {
            (null, null) => null,
            (int first, int second) => Math.Min(first, second) + 1,
            (int first, null) => first + 1,
            (null, int second) => second,
        };

        // Ensure it is within range, default to 32 if null
        DeleteCount = Math.Min(candidateIndex ?? 30, Content.Words.Length);
    }

    private async Task CancelAsync()
    {
        var result = new DeleteWordsDialogResult([]);
        await Dialog.CancelAsync(result);
    }

    private async Task ConfirmAsync()
    {
        int pageNumber = FirstWordReference.PageNumber;
        var page = Content.EditionState.LoadedPages[pageNumber].Page;
        var wordReferences = Content.Words
            .Select(x => x.Key)
            .Where(x => x != null && x.PageNumber == pageNumber)
            .Select(x => new { WordReference = x, Element = page.Words[x.WordIndex]!.Elements[0] });

        WordReference[] wordsToDelete = Position switch {
            DeleteWordsPosition.Left => wordReferences.Where(x => x.Element.Bounds.X <= DeletePosition).Select(x => x.WordReference).ToArray(),
            DeleteWordsPosition.Right => wordReferences.Where(x => x.Element.Bounds.GetRight() >= DeletePosition).Select(x => x.WordReference).ToArray(),
            _ => Content.Words.Take(DeleteCount).Select(x => x.Key).ToArray(),
        };

        var result = new DeleteWordsDialogResult(wordsToDelete);
        await Dialog.CloseAsync(result);
    }

    private void PositionTypeChanged()
    {
        if (FirstElement == null) return;
        DeletePosition = Position switch {
            DeleteWordsPosition.Left => FirstElement.Bounds.GetRight(),
            DeleteWordsPosition.Right => FirstElement.Bounds.X,
            _ => DeletePosition
        };
    }
        

    private void SetDeleteCount(int count)
    {
        DeleteCount = count;
    }

    private enum DeleteWordsPosition
    {
        Sequential,
        Left,
        Right
    }
}