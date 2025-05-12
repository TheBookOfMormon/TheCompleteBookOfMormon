using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace WordsAnalysis.Components;

public partial class ShortcutHandler
{
    private readonly HashSet<IShortcutHandler> ShortcutHandlers = new();
    private int SuspendEventsCount;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }


    public void RegisterHandler(IShortcutHandler handler)
    {
        ShortcutHandlers.Add(handler);
    }

    public void ResumeEvents()
    {
        Interlocked.Decrement(ref SuspendEventsCount);
    }

    public void SuspendEvents()
    {
        Interlocked.Increment(ref SuspendEventsCount);
    }

    public void UnregisterHandler(IShortcutHandler handler)
    {
        ShortcutHandlers.Remove(handler);
    }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (SuspendEventsCount != 0) return;
        Shortcut? shortcut = Shortcut.FromKeyboardEventArgs(e);
        if (shortcut == null) return;

        IShortcutHandler? handler = ShortcutHandlers.FirstOrDefault(x => x.Enabled && x.Shortcut == shortcut);
        if (handler != null)
        {
            await handler.HandleShortcut(shortcut.Value);
        }
    }


}

public interface IShortcutHandler
{
    bool Enabled { get; }
    Shortcut? Shortcut { get; }
    Task HandleShortcut(Shortcut shortcut);
}

public enum SpecialKey
{
    None, Alt, Shift, Control
}

public readonly record struct Shortcut(SpecialKey SpecialKey, string Code)
{
    public static readonly ShortcutBuilder Alt = new ShortcutBuilder(SpecialKey.Alt);
    public static readonly ShortcutBuilder Control = new ShortcutBuilder(SpecialKey.Control);
    public static readonly ShortcutBuilder Shift = new ShortcutBuilder(SpecialKey.Shift);

    public static readonly Shortcut A = new Shortcut(SpecialKey.None, "A");
    public static readonly Shortcut B = new Shortcut(SpecialKey.None, "B");
    public static readonly Shortcut C = new Shortcut(SpecialKey.None, "C");
    public static readonly Shortcut D = new Shortcut(SpecialKey.None, "D");
    public static readonly Shortcut E = new Shortcut(SpecialKey.None, "E");
    public static readonly Shortcut F = new Shortcut(SpecialKey.None, "F");
    public static readonly Shortcut G = new Shortcut(SpecialKey.None, "G");
    public static readonly Shortcut H = new Shortcut(SpecialKey.None, "H");
    public static readonly Shortcut I = new Shortcut(SpecialKey.None, "I");
    public static readonly Shortcut J = new Shortcut(SpecialKey.None, "J");
    public static readonly Shortcut K = new Shortcut(SpecialKey.None, "K");
    public static readonly Shortcut L = new Shortcut(SpecialKey.None, "L");
    public static readonly Shortcut M = new Shortcut(SpecialKey.None, "M");
    public static readonly Shortcut N = new Shortcut(SpecialKey.None, "N");
    public static readonly Shortcut O = new Shortcut(SpecialKey.None, "O");
    public static readonly Shortcut P = new Shortcut(SpecialKey.None, "P");
    public static readonly Shortcut Q = new Shortcut(SpecialKey.None, "Q");
    public static readonly Shortcut R = new Shortcut(SpecialKey.None, "R");
    public static readonly Shortcut S = new Shortcut(SpecialKey.None, "S");
    public static readonly Shortcut T = new Shortcut(SpecialKey.None, "T");
    public static readonly Shortcut U = new Shortcut(SpecialKey.None, "U");
    public static readonly Shortcut V = new Shortcut(SpecialKey.None, "V");
    public static readonly Shortcut W = new Shortcut(SpecialKey.None, "W");
    public static readonly Shortcut X = new Shortcut(SpecialKey.None, "X");
    public static readonly Shortcut Y = new Shortcut(SpecialKey.None, "Y");
    public static readonly Shortcut Z = new Shortcut(SpecialKey.None, "Z");
    public static readonly Shortcut Up = new Shortcut(SpecialKey.None, "ARROWUP");
    public static readonly Shortcut Down = new Shortcut(SpecialKey.None, "ARROWDOWN");
    public static readonly Shortcut Left = new Shortcut(SpecialKey.None, "ARROWLEFT");
    public static readonly Shortcut Right = new Shortcut(SpecialKey.None, "ARROWRIGHT");

    public static Shortcut? FromKeyboardEventArgs(KeyboardEventArgs e)
    {
        int specialKeyCount = 0;
        if (e.CtrlKey) specialKeyCount++;
        if (e.ShiftKey) specialKeyCount++;
        if (e.AltKey) specialKeyCount++;
        if (specialKeyCount > 1) return null;

        SpecialKey specialKey = e switch {
            {  CtrlKey : true } => SpecialKey.Control,
            {  ShiftKey : true } => SpecialKey.Shift,
            {  AltKey : true } => SpecialKey.Alt,
            _ => SpecialKey.None
        };

        return new Shortcut(specialKey, e.Key.ToUpper());
    }

    public string GetDescription()
    {
        string specialKey = SpecialKey switch {
            SpecialKey.None => "",
            SpecialKey.Alt => "ALT ",
            SpecialKey.Control => "CTRL ",
            SpecialKey.Shift => "SHIFT ",
            _ => throw new NotImplementedException(SpecialKey.ToString())
        };
        return $"({specialKey}{Code})";
    }
}

public class ShortcutBuilder
{
    public readonly Shortcut A;
    public readonly Shortcut B;
    public readonly Shortcut C;
    public readonly Shortcut D;
    public readonly Shortcut E;
    public readonly Shortcut F;
    public readonly Shortcut G;
    public readonly Shortcut H;
    public readonly Shortcut I;
    public readonly Shortcut J;
    public readonly Shortcut K;
    public readonly Shortcut L;
    public readonly Shortcut M;
    public readonly Shortcut N;
    public readonly Shortcut O;
    public readonly Shortcut P;
    public readonly Shortcut Q;
    public readonly Shortcut R;
    public readonly Shortcut S;
    public readonly Shortcut T;
    public readonly Shortcut U;
    public readonly Shortcut V;
    public readonly Shortcut W;
    public readonly Shortcut X;
    public readonly Shortcut Y;
    public readonly Shortcut Z;
    public readonly Shortcut Up;
    public readonly Shortcut Down;
    public readonly Shortcut Left;
    public readonly Shortcut Right;

    private readonly SpecialKey SpecialKey;

    public ShortcutBuilder(SpecialKey specialKey)
    {
        SpecialKey = specialKey;
        A = new Shortcut(SpecialKey, "A");
        B = new Shortcut(SpecialKey, "B");
        C = new Shortcut(SpecialKey, "C");
        D = new Shortcut(SpecialKey, "D");
        E = new Shortcut(SpecialKey, "E");
        F = new Shortcut(SpecialKey, "F");
        G = new Shortcut(SpecialKey, "G");
        H = new Shortcut(SpecialKey, "H");
        I = new Shortcut(SpecialKey, "I");
        J = new Shortcut(SpecialKey, "J");
        K = new Shortcut(SpecialKey, "K");
        L = new Shortcut(SpecialKey, "L");
        M = new Shortcut(SpecialKey, "M");
        N = new Shortcut(SpecialKey, "N");
        O = new Shortcut(SpecialKey, "O");
        P = new Shortcut(SpecialKey, "P");
        Q = new Shortcut(SpecialKey, "Q");
        R = new Shortcut(SpecialKey, "R");
        S = new Shortcut(SpecialKey, "S");
        T = new Shortcut(SpecialKey, "T");
        U = new Shortcut(SpecialKey, "U");
        V = new Shortcut(SpecialKey, "V");
        W = new Shortcut(SpecialKey, "W");
        X = new Shortcut(SpecialKey, "X");
        Y = new Shortcut(SpecialKey, "Y");
        Z = new Shortcut(SpecialKey, "Z");
        Up = new Shortcut(SpecialKey, "ARROWUP");
        Down = new Shortcut(SpecialKey, "ARROWDOWN");
        Left = new Shortcut(SpecialKey, "ARROWLEFT");
        Right = new Shortcut(SpecialKey, "ARROWRIGHT");
    }
}


