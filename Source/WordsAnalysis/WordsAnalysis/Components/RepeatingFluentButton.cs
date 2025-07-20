using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsAnalysis.Components;

public class RepeatingFluentButton : FluentButton, IAsyncDisposable
{
    private bool IsDown;
    private bool IsDisposed;

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
        IsDisposed = true;
        await base.DisposeAsync();
    }

    private async Task MouseDownAsync(MouseEventArgs e)
    {
        if (e.Button == 0)
        {
            IsDown = true;
            await Task.Delay(500);
            while (IsDown && !IsDisposed)
            {
                await OnClick.InvokeAsync(e);
                await Task.Delay(100);
            }
        }
    }

    private void MouseUp(MouseEventArgs e)
    {
        if (e.Button == 0)
        {
            IsDown = false;
        };
    }
}
