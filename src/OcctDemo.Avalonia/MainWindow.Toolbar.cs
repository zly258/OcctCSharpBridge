using Avalonia;
using Avalonia.Controls;
using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private void BuildToolbar()
    {
        var selectedIndex = _selectionCombo?.SelectedIndex ?? 0;
        _toolbar.Children.Clear();
        _toolbar.Children.Add(AsyncToolButton(DemoLocalization.Text("Toolbar.New"), NewDocumentAsync));
        _toolbar.Children.Add(AsyncToolButton(DemoLocalization.Text("Toolbar.Open"), OpenDocumentAsync));
        _toolbar.Children.Add(AsyncToolButton(DemoLocalization.Text("Toolbar.Save"), () => SaveDocumentAsync(false)));
        _toolbar.Children.Add(ToolSeparator());
        _undoButton = ToolButton(DemoLocalization.Text("Toolbar.Undo"), Undo);
        _redoButton = ToolButton(DemoLocalization.Text("Toolbar.Redo"), Redo);
        _toolbar.Children.Add(_undoButton);
        _toolbar.Children.Add(_redoButton);
        _toolbar.Children.Add(ToolSeparator());
        _toolbar.Children.Add(ToolButton(DemoLocalization.Text("Toolbar.Shaded"), () => ApplyMenuVisualStyle(DemoVisualStyle.Shaded)));
        _toolbar.Children.Add(ToolButton(DemoLocalization.Text("Toolbar.Wireframe"), () => ApplyMenuVisualStyle(DemoVisualStyle.Wireframe)));
        _toolbar.Children.Add(ToolButton(DemoLocalization.Text("Toolbar.Extents"), () => Session.Engine.FitAll()));
        _toolbar.Children.Add(ToolButton(DemoLocalization.Text("Toolbar.Isometric"), () => Session.Engine.SetView(OcctViewOrientation.Isometric)));
        _toolbar.Children.Add(ToolSeparator());
        _toolbar.Children.Add(new TextBlock
        {
            Text = DemoLocalization.Text("Toolbar.Selection"),
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(6, 0, 2, 0)
        });
        _selectionCombo = new ComboBox
        {
            ItemsSource = Enum.GetValues<OcctSelectionMode>().Select(SelectionModeName).ToArray()
        };
        _selectionCombo.SelectedIndex = Math.Clamp(selectedIndex, 0, Enum.GetValues<OcctSelectionMode>().Length - 1);
        _selectionCombo.SelectionChanged += (_, _) =>
        {
            if (_selectionCombo.SelectedIndex >= 0)
                SetSelectionMode((OcctSelectionMode)_selectionCombo.SelectedIndex);
        };
        _toolbar.Children.Add(_selectionCombo);
        UpdateHistoryUi();
    }
}
