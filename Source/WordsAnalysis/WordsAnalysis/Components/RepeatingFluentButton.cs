using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;

namespace WordsAnalysis.Components;

public class RepeatingFluentButton : FluentButton, IAsyncDisposable
{
    private CancellationTokenSource? MouseDownCancellationTokenSource;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "span");
        builder.AddAttribute(2, "onmousedown", EventCallback.Factory.Create<MouseEventArgs>(this, MouseDownAsync));
        builder.AddAttribute(3, "onmouseup", EventCallback.Factory.Create<MouseEventArgs>(this, MouseUp));
        base.BuildRenderTree(builder);
        builder.CloseElement();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        MouseDownCancellationTokenSource?.Cancel();
        await base.DisposeAsync();
    }

    private async Task MouseDownAsync(MouseEventArgs e)
    {
        if (e.Button == 0)
        {
            MouseDownCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = MouseDownCancellationTokenSource.Token;
            await Task.Delay(500, cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                await OnClick.InvokeAsync(e);
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    private void MouseUp(MouseEventArgs e)
    {
        if (e.Button == 0)
        {
            var source = MouseDownCancellationTokenSource;
            MouseDownCancellationTokenSource = null;
            source?.Cancel();
        }
        ;
    }
}
