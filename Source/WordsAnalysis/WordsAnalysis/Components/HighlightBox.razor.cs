using Microsoft.AspNetCore.Components;

namespace WordsAnalysis.Components;
public partial class HighlightBox
{
    [EditorRequired, Parameter]
    public required int X { get; set; }

    [EditorRequired, Parameter]
    public required int Y { get; set; }

    [EditorRequired, Parameter]
    public required int Width { get; set; }

    [EditorRequired, Parameter]
    public required int Height { get; set; }

}