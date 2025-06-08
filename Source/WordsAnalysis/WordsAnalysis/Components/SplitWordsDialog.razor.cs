using DocumentsModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Features.SyncDocuments;

namespace WordsAnalysis.Components;
public partial class SplitWordsDialog
{
    public record SplitWordsDialogContent(EditionState Edition, WordReference WordReference, SplitWordSuggestion[] Suggestions, int PageWidth, int PageHeight);
    public record SplitWordSuggestion(SplitWord[] Words);
    public record SplitWord(string Text, OcrRect Bounds);

    private int SelectedSuggestionIndex = 0;

    public record SplitWordsDialogResult(SplitWordSuggestion? Suggestion);

    [Parameter]
    public SplitWordsDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private async Task CancelAsync()
    {
        var result = new SplitWordsDialogResult(null);
        await Dialog.CancelAsync(result);
    }

    private async Task ConfirmAsync()
    {
        SplitWordSuggestion suggestion = Content.Suggestions[SelectedSuggestionIndex];
        var result = new SplitWordsDialogResult(suggestion);
        await Dialog.CloseAsync(result);
    }

    private string GetSuggestionClass(int index)
    {
        return index == SelectedSuggestionIndex ? "--selected" : "";
    }

    private void SelectSuggestion(int index)
    {
        SelectedSuggestionIndex = index;
    }
}