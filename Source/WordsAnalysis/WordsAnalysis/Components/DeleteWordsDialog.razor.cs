using DocumentsModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Features.SyncDocuments;

namespace WordsAnalysis.Components;

public partial class DeleteWordsDialog
{
    public record DeleteWordsDialogContent(KeyValuePair<WordReference, string?>[] Words);
    public record DeleteWordsDialogResult(WordReference[] DeletedWords);

    [Parameter]
    public DeleteWordsDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private int DeleteCount;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        DeleteCount = Math.Min(30, Content.Words.Length);
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