using OcctNet;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private bool _enhancementsWired;

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        if (_enhancementsWired) return;
        _enhancementsWired = true;

        Viewport.ObjectSelectionChanged += (_, args) => Dispatcher.InvokeAsync(() =>
        {
            if (_session is null || args.SelectedObjects.Count <= 1) return;
            _session.ActiveObject = null;
            ShowSelectionProperties(args.SelectedObjects);
        });
    }
}
