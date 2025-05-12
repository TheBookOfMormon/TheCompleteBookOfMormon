using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace WordsAnalysis.Components;

public partial class IconPanelButton<Icon> : IShortcutHandler, IDisposable
{
    [Parameter]
    public string? AccessKey { get; set; }

    [Parameter]
    public string Id { get; set; } = $"x{Guid.NewGuid()}";

    [Parameter]
    public required bool Enabled { get; set; } = true;

    [Parameter, EditorRequired]
    public required EventCallback OnClick { get; set; }

    [Parameter]
    public Shortcut? Shortcut { get; set; }

    [CascadingParameter]
    private ShortcutHandler ShortcutHandler { get; set; } = null!;

    [Parameter, EditorRequired]
    public required string Tooltip { get; set; }

    public string ButtonId => $"{Id}-button";

    private FluentButton ButtonElement = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ShortcutHandler.RegisterHandler(this);
    }

    private string GetTooltipText()
    {
        string? shortcut = Shortcut == null ? null : $" {Shortcut.Value.GetDescription()}";
        return $"{Tooltip}{shortcut}";
    }

    async Task IShortcutHandler.HandleShortcut(Shortcut shortcut)
    {
        if (Enabled)
        {
            //await ButtonElement!.FocusAsync();
            await OnClick.InvokeAsync();
        }
    }

    void IDisposable.Dispose()
    {
        ShortcutHandler?.UnregisterHandler(this);
    }
}