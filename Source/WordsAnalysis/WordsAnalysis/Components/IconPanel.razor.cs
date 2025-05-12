using Microsoft.AspNetCore.Components;

namespace WordsAnalysis.Components;
public partial class IconPanel<Icon>
{
    [Parameter]
    public required RenderFragment ChildContent { get; set; }

    [Parameter]
    public string Id { get; set; } = $"x{Guid.NewGuid()}";

    [Parameter, EditorRequired]
    public required string Name { get; set; }

    private string IconId => $"{Id}-icon";
}

