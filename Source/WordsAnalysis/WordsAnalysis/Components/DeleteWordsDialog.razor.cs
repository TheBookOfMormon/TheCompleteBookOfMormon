using DocumentsModel;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Features.SyncDocuments;

namespace WordsAnalysis.Components;

public partial class DeleteWordsDialog
{
    public record DeleteWordsDialogContent(EditionState EditionState, KeyValuePair<WordReference, string?>[] Words);
    public record DeleteWordsDialogResult(WordReference[] DeletedWords);

    [Parameter]
    public DeleteWordsDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private int DeleteCount;

    protected override void OnInitialized()
    {
        base.OnInitialized();

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
        OcrWord firstWord = Content.Words[0].Key.GetWord(Content.EditionState)!;
        OcrElement firstElement = firstWord.Elements.Last();
        // Try to find start of next column
        for (int index = 2; index < Content.Words.Length; index++)
        {
            OcrWord? candidateWord = Content.Words[index].Key.GetWord(Content.EditionState);
            if (candidateWord is null) continue;

            OcrElement candidateElement = candidateWord.Elements.Last();
            if (candidateElement.Bounds.Y + candidateElement.Bounds.Height < firstElement.Bounds.Y)
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
            (null, int second) => second + 1,
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
        var result = new DeleteWordsDialogResult(Content.Words.Take(DeleteCount).Select(x => x.Key).ToArray());
        await Dialog.CloseAsync(result);
    }

    private void SetDeleteCount(int count)
    {
        DeleteCount = count;
    }
}