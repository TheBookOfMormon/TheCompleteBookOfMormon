using DocumentsModel;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Features.SyncDocuments;

namespace WordsAnalysis.Components;
public partial class SplitWordsDialog
{
    public record SplitWordsDialogContent(EditionState Edition, WordReference WordReference, SplitWordSuggestion[] Suggestions, int PageWidth, int PageHeight);
    public record SplitWordSuggestion(SplitWord[] Words);
    public record SplitWord(string Text, OcrRect Bounds);

    public record SplitWordsDialogResult();

    [Parameter]
    public SplitWordsDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

}