using OcctDemo.Common;
using OcctNet;
using Controls = System.Windows.Controls;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private void BuildToolbar()
    {
        var selectedIndex = _selectionCombo?.SelectedIndex ?? 0;
        MainToolBar.Items.Clear();
        MainToolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.New"), (_, _) => NewDocument()));
        MainToolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Open"), (_, _) => OpenDocument()));
        MainToolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Save"), (_, _) => SaveDocument(false)));
        MainToolBar.Items.Add(new Controls.Separator());
        _undoButton = ToolButton(DemoLocalization.Text("Toolbar.Undo"), (_, _) => Undo());
        _redoButton = ToolButton(DemoLocalization.Text("Toolbar.Redo"), (_, _) => Redo());
        MainToolBar.Items.Add(_undoButton);
        MainToolBar.Items.Add(_redoButton);
        MainToolBar.Items.Add(new Controls.Separator());
        MainToolBar.Items.Add(VisualStyleButton(DemoLocalization.Text("Toolbar.ShadedEdges"), DemoVisualStyle.ShadedEdges));
        MainToolBar.Items.Add(VisualStyleButton(DemoLocalization.Text("Toolbar.Shaded"), DemoVisualStyle.Shaded));
        MainToolBar.Items.Add(VisualStyleButton(DemoLocalization.Text("Toolbar.Wireframe"), DemoVisualStyle.Wireframe));
        MainToolBar.Items.Add(VisualStyleButton(DemoLocalization.Text("Toolbar.Hlr"), DemoVisualStyle.HiddenLine));
        MainToolBar.Items.Add(new Controls.Separator());
        MainToolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.FitAll"), (_, _) => Session.Engine.FitAll()));
        MainToolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric)));
        MainToolBar.Items.Add(new Controls.Separator());
        MainToolBar.Items.Add(new Controls.TextBlock
        {
            Text = DemoLocalization.Text("Toolbar.Selection"),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Margin = new System.Windows.Thickness(6, 0, 2, 0)
        });
        _selectionCombo = new Controls.ComboBox { Width = 125, Margin = new System.Windows.Thickness(2) };
        foreach (var mode in Enum.GetValues<OcctSelectionMode>()) _selectionCombo.Items.Add(SelectionModeName(mode));
        _selectionCombo.SelectedIndex = Math.Clamp(selectedIndex, 0, _selectionCombo.Items.Count - 1);
        _selectionCombo.SelectionChanged += (_, _) =>
        {
            if (_selectionCombo.SelectedIndex >= 0) SetSelectionMode((OcctSelectionMode)_selectionCombo.SelectedIndex);
        };
        MainToolBar.Items.Add(_selectionCombo);
        UpdateHistoryUi();
    }

    private Controls.Button VisualStyleButton(string text, DemoVisualStyle style)
    {
        var button = ToolButton(text, (_, _) => ApplyMenuVisualStyle(style));
        if (_visualStyle == style)
        {
            button.FontWeight = System.Windows.FontWeights.SemiBold;
            button.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(214, 231, 247));
            button.BorderBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(120, 164, 202));
        }
        return button;
    }
}
