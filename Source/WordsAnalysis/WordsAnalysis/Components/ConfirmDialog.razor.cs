using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace WordsAnalysis.Components;

public partial class ConfirmDialog
{
    [Parameter]
    public string Title { get; set; } = "Confirm";
    
    [EditorRequired, Parameter]
    public required string Message { get; set; }

    [Parameter]
    public string ConfirmButtonText { get; set; } = "Yes";
    
    [Parameter]
    public string CancelButtonText { get; set; } = "No";

    [Parameter]
    public ConfirmDialogContent Content { get; set; } = ConfirmDialogContent.Empty;

    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;

    private async Task ConfirmAsync(bool confirmed)
    {
        await Dialog.CloseAsync(confirmed);
    }
}

public readonly record struct ConfirmDialogContent(string Message)
{
    public readonly static ConfirmDialogContent Empty = new ("");
}