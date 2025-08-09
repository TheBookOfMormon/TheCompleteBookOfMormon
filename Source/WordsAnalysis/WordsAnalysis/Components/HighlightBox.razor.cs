using DocumentsModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace WordsAnalysis.Components;

public partial class HighlightBox : IAsyncDisposable
{
    [Parameter]
    public OcrRect Rect { get; set; } = OcrRect.Empty;

    [Parameter]
    public EventCallback<OcrRect> RectChanged { get; set; }

    private IJSObjectReference? JSModule;
    private DotNetObjectReference<InteropCallbacks>? DotNetRef;

    private readonly InteropCallbacks JSCallbacks;

    private const int DragHandlePixelSize = 16;
    private const int DragHandlesCombinedPixelSize = DragHandlePixelSize * 2;

    private bool IsInteracting;
    private CellPosition CurrentCellPosition;
    private ElementReference HighlightBoxElementReference;
    private int MouseDownX;
    private int MouseDownY;
    private OcrRect MouseDownRect = OcrRect.Empty;

    private enum CellPosition
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Middle,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }

    public HighlightBox()
    {
        JSCallbacks = new InteropCallbacks(this);
    }

    private string GetStyle() =>
        $"""
            left:{Rect.X - DragHandlePixelSize}px;
            top:{Rect.Y - DragHandlePixelSize}px;
            width:{Rect.Width + DragHandlesCombinedPixelSize}px;
            height:{Rect.Height + DragHandlesCombinedPixelSize}px;
            grid-template-columns: {DragHandlePixelSize}px 1fr {DragHandlePixelSize}px;
            grid-template-rows: {DragHandlePixelSize}px 1fr {DragHandlePixelSize}px;
            min-width: {DragHandlesCombinedPixelSize}px; 
            min-height: {DragHandlesCombinedPixelSize}px;
        """;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            JSModule = await JS.InvokeAsync<IJSObjectReference>("import", "/HighlightBox.js");
            DotNetRef = DotNetObjectReference.Create(JSCallbacks);
            await JSModule.InvokeVoidAsync("initialize", HighlightBoxElementReference);
        }
    }

    private async Task MouseDownAsync(MouseEventArgs e, CellPosition cellPosition)
    {
        if (e.Button != 0) return;

        CurrentCellPosition = cellPosition;
        MouseDownX = (int)Math.Truncate(e.ClientX);
        MouseDownY = (int)Math.Truncate(e.ClientY);
        MouseDownRect = Rect.Normalize();
        IsInteracting = true;

        if (JSModule is not null && DotNetRef is not null)
            await JSModule.InvokeVoidAsync("startInteraction", DotNetRef);
    }

    private async Task OnMouseMove(int clientX, int clientY)
    {
        if (!IsInteracting) return;

        int xOffset = clientX - MouseDownX;
        int yOffset = clientY - MouseDownY;
        if (Math.Abs(xOffset) < 1 && Math.Abs(yOffset) < 1)
            return;

        OcrRect newRect = MouseDownRect;

        if (CurrentCellPosition is CellPosition.TopLeft or CellPosition.Left or CellPosition.BottomLeft)
            newRect = newRect.MoveX(MouseDownRect.X + xOffset);

        if (CurrentCellPosition is CellPosition.TopLeft or CellPosition.Top or CellPosition.TopRight)
            newRect = newRect.MoveY(MouseDownRect.Y + yOffset);

        if (CurrentCellPosition is CellPosition.TopRight or CellPosition.Right or CellPosition.BottomRight)
            newRect = newRect with { Width = MouseDownRect.Width + xOffset };

        if (CurrentCellPosition is CellPosition.BottomLeft or CellPosition.Bottom or CellPosition.BottomRight)
            newRect = newRect with { Height = MouseDownRect.Height + yOffset };

        if (CurrentCellPosition == CellPosition.Middle)
            newRect = newRect with { X = MouseDownRect.X + xOffset, Y = MouseDownRect.Y + yOffset };

        newRect = newRect.Normalize();
        await RectChanged.InvokeAsync(newRect);
    }

    private Task OnMouseUp()
    {
        IsInteracting = false;
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        DotNetRef?.Dispose();
        if (JSModule is not null)
            await JSModule.DisposeAsync();
    }

    private class InteropCallbacks
    {
        private readonly HighlightBox Owner;

        public InteropCallbacks(HighlightBox owner)
        {
            Owner = owner;
        }

        [JSInvokable]
        public async Task OnMouseMove(int clientX, int clientY) => 
            await Owner.OnMouseMove(clientX, clientY);

        [JSInvokable]
        public async Task OnMouseUp() => await Owner.OnMouseUp();
    }
}
