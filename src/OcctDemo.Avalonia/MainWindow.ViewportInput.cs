using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private async void ViewportPreviewKeyInput(object? sender, OcctKeyInputEventArgs e)
    {
        if (_session is null) return;

        var shortcut = DemoViewportShortcuts.GetShortcut(e);
        if (shortcut == DemoViewportShortcut.None) return;
        e.Handled = true;

        try
        {
            switch (shortcut)
            {
                case DemoViewportShortcut.Undo:
                    Undo();
                    break;
                case DemoViewportShortcut.Redo:
                    Redo();
                    break;
                case DemoViewportShortcut.NewDocument:
                    await NewDocumentAsync();
                    break;
                case DemoViewportShortcut.OpenDocument:
                    await OpenDocumentAsync();
                    break;
                case DemoViewportShortcut.SaveDocument:
                    await SaveDocumentAsync(false);
                    break;
                case DemoViewportShortcut.SaveDocumentAs:
                    await SaveDocumentAsync(true);
                    break;
                case DemoViewportShortcut.Delete:
                    await RunCommandAsync(DemoCommandId.Delete);
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
            }
        }
        catch (Exception exception)
        {
            ExecuteSafe(() => throw exception);
        }
    }
}
