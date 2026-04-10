using DocumentsModel;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Features.SyncDocuments;

namespace WordsAnalysis.Components;
public partial class SplitWordsDialog
{

    [Parameter]
    public SplitWordsDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private KeyValuePair<int, SplitWordSuggestion>[] Data = null!;
    private KeyValuePair<int, SplitWordSuggestion> SelectedSuggestion;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Data = Content.Suggestions.Select((suggestion, i) => new KeyValuePair<int, SplitWordSuggestion>(i, suggestion)).ToArray();
        SelectedSuggestion = new KeyValuePair<int, SplitWordSuggestion>(0, Content.Suggestions[0]);
    }

    private async Task CancelAsync()
    {
        var result = new SplitWordsDialogResult(null);
        await Dialog.CancelAsync(result);
    }

    private async Task ConfirmAsync()
    {
        var result = new SplitWordsDialogResult(SelectedSuggestion.Value);
        await Dialog.CloseAsync(result);
    }

    private string GetOptionText(KeyValuePair<int, SplitWordSuggestion> item)
    {
        return string.Join(' ', item.Value.Words.Select(x => x.Text));
    }
}