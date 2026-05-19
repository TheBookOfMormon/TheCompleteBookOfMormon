using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace WordsAnalysis.Components;

public partial class SaveChangesDialog
{
    [Parameter]
    public SaveChangesDialogContent Content { get; set; } = SaveChangesDialogContent.Empty;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private async Task CloseAsync(SaveChangesDialogResult result)
    {
        await Dialog.CloseAsync(result);
    }
}

public readonly record struct SaveChangesDialogContent(string Message)
{
    public readonly static SaveChangesDialogContent Empty = new("");
}

public enum SaveChangesDialogResult
{
    Yes,
    No,
    Abort
}
