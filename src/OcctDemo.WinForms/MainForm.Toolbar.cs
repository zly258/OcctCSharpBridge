using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    private void BuildToolBar()
    {
        var selectedIndex = Math.Max(_selectionCombo.SelectedIndex, 0);
        _toolBar.Items.Clear();
        _selectionCombo.Items.Clear();
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.New"), (_, _) => NewDocument()));
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Open"), (_, _) => OpenDocument()));
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Save"), (_, _) => SaveDocument(false)));
        _toolBar.Items.Add(new ToolStripSeparator());
        _undoButton = ToolButton(DemoLocalization.Text("Toolbar.Undo"), (_, _) => Undo());
        _redoButton = ToolButton(DemoLocalization.Text("Toolbar.Redo"), (_, _) => Redo());
        _toolBar.Items.Add(_undoButton);
        _toolBar.Items.Add(_redoButton);
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(VisualStyleButton(DemoLocalization.Text("Toolbar.ShadedEdges"), DemoVisualStyle.ShadedEdges));
        _toolBar.Items.Add(VisualStyleButton(DemoLocalization.Text("Toolbar.Shaded"), DemoVisualStyle.Shaded));
        _toolBar.Items.Add(VisualStyleButton(DemoLocalization.Text("Toolbar.Wireframe"), DemoVisualStyle.Wireframe));
        _toolBar.Items.Add(VisualStyleButton(DemoLocalization.Text("Toolbar.Hlr"), DemoVisualStyle.HiddenLine));
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.FitAll"), (_, _) => Session.Engine.FitAll()));
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric)));
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(new ToolStripLabel(DemoLocalization.Text("Toolbar.Selection")));
        foreach (var mode in Enum.GetValues<OcctSelectionMode>()) _selectionCombo.Items.Add(SelectionModeName(mode));
        _selectionCombo.SelectedIndex = Math.Min(selectedIndex, _selectionCombo.Items.Count - 1);
        _toolBar.Items.Add(_selectionCombo);
        UpdateHistoryUi();
    }

    private ToolStripButton VisualStyleButton(string text, DemoVisualStyle style)
    {
        var button = ToolButton(text, (_, _) => ApplyMenuVisualStyle(style));
        button.CheckOnClick = false;
        button.Checked = _visualStyle == style;
        return button;
    }
}
