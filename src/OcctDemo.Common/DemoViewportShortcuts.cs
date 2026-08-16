using OcctNet;

namespace OcctDemo.Common;

public enum DemoViewportShortcut
{
    None = 0,
    Undo,
    Redo,
    NewDocument,
    OpenDocument,
    SaveDocument,
    SaveDocumentAs,
    Delete,
    FitAll,
    IsometricView,
    FrontView,
    LeftView,
    TopView,
    ClearSelection
}

public static class DemoViewportShortcuts
{
    public static DemoViewportShortcut GetShortcut(OcctKeyInputEventArgs input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Kind != OcctKeyInputKind.Pressed || input.IsRepeat) return DemoViewportShortcut.None;

        var control = input.Modifiers.HasFlag(OcctInputModifiers.Control);
        var shift = input.Modifiers.HasFlag(OcctInputModifiers.Shift);
        return (control, shift, input.Key) switch
        {
            (true, _, OcctKey.Z) => DemoViewportShortcut.Undo,
            (true, _, OcctKey.Y) => DemoViewportShortcut.Redo,
            (true, _, OcctKey.N) => DemoViewportShortcut.NewDocument,
            (true, _, OcctKey.O) => DemoViewportShortcut.OpenDocument,
            (true, true, OcctKey.S) => DemoViewportShortcut.SaveDocumentAs,
            (true, false, OcctKey.S) => DemoViewportShortcut.SaveDocument,
            (false, _, OcctKey.Delete) => DemoViewportShortcut.Delete,
            (false, _, OcctKey.F) => DemoViewportShortcut.FitAll,
            (false, _, OcctKey.D0) => DemoViewportShortcut.IsometricView,
            (false, _, OcctKey.D1) => DemoViewportShortcut.FrontView,
            (false, _, OcctKey.D2) => DemoViewportShortcut.LeftView,
            (false, _, OcctKey.D3) => DemoViewportShortcut.TopView,
            (false, _, OcctKey.Escape) => DemoViewportShortcut.ClearSelection,
            _ => DemoViewportShortcut.None
        };
    }
}
