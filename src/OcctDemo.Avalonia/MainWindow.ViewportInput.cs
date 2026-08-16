using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private void ViewportPreviewKeyInput(object? sender, OcctKeyInputEventArgs e)
    {
        if (_session is null) return;

        switch (DemoViewportShortcuts.GetShortcut(e))
        {
            case DemoViewportShortcut.Undo:
                Undo();
                break;
            case DemoViewportShortcut.Redo:
                Redo();
                break;
            case DemoViewportShortcut.NewDocument:
                NewDocument();
                break;
            case DemoViewportShortcut.OpenDocument:
                _ = OpenDocumentAsync();
                break;
            case DemoViewportShortcut.SaveDocument:
                _ = SaveDocumentAsync(false);
                break;
            case DemoViewportShortcut.SaveDocumentAs:
                _ = SaveDocumentAsync(true);
                break;
            case DemoViewportShortcut.Delete:
                _ = RunCommandAsync(DemoCommandId.Delete);
                break;
            case DemoViewportShortcut.FitAll:
                Session.Engine.FitAll();
                break;
            case DemoViewportShortcut.IsometricView:
                Session.Engine.SetView(OcctViewOrientation.Isometric);
                break;
            case DemoViewportShortcut.FrontView:
                Session.Engine.SetView(OcctViewOrientation.Front);
                break;
            case DemoViewportShortcut.LeftView:
                Session.Engine.SetView(OcctViewOrientation.Left);
                break;
            case DemoViewportShortcut.TopView:
                Session.Engine.SetView(OcctViewOrientation.Top);
                break;
            case DemoViewportShortcut.ClearSelection:
                Session.Engine.ClearSelection();
                _viewport.RaiseSelectionChanged();
                break;
            default:
                return;
        }

        e.Handled = true;
    }
}
