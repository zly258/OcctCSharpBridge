using System.Globalization;
using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    private void BuildMenus()
    {
        _menu.Items.Clear();

        var file = new ToolStripMenuItem(DemoLocalization.Text("Menu.File"));
        file.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.New"), (_, _) => NewDocument(), "Ctrl+N"));
        file.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Open"), (_, _) => OpenDocument(), "Ctrl+O"));
        file.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Save"), (_, _) => SaveDocument(false), "Ctrl+S"));
        file.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.SaveAs"), (_, _) => SaveDocument(true), "Ctrl+Shift+S"));
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Import"), (_, _) => ImportDocument()));
        file.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.ExportSelected"), (_, _) => ExportSelected()));
        file.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.ExportImage"), (_, _) => ExportViewImage()));
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Exit"), (_, _) => Close(), "Alt+F4"));

        var edit = new ToolStripMenuItem(DemoLocalization.Text("Menu.Edit"));
        _undoMenuItem = MenuItem(DemoLocalization.Text("Menu.Undo"), (_, _) => Undo(), "Ctrl+Z");
        _redoMenuItem = MenuItem(DemoLocalization.Text("Menu.Redo"), (_, _) => Redo(), "Ctrl+Y");
        edit.DropDownItems.Add(_undoMenuItem);
        edit.DropDownItems.Add(_redoMenuItem);
        edit.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(edit, DemoCommandId.Translate, DemoCommandId.Rotate, DemoCommandId.Scale, DemoCommandId.Mirror, DemoCommandId.Copy);
        edit.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(edit, DemoCommandId.Delete);
        edit.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.ClearSelection"), (_, _) => { Session.Engine.ClearSelection(); _viewport.RaiseSelectionChanged(); }));
        edit.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.ShowAll"), (_, _) => Session.Engine.ShowAll()));
        edit.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.HideAll"), (_, _) => Session.Engine.HideAll()));

        var draw = new ToolStripMenuItem(DemoLocalization.Text("Menu.Draw"));
        AddCommands(draw, DemoCommandId.Point, DemoCommandId.Line, DemoCommandId.Polyline);
        draw.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(draw, DemoCommandId.Circle, DemoCommandId.ArcThreePoints, DemoCommandId.ArcCenter, DemoCommandId.Ellipse);
        draw.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(draw, DemoCommandId.Rectangle, DemoCommandId.Polygon, DemoCommandId.Bezier, DemoCommandId.BSpline);

        var solid = new ToolStripMenuItem(DemoLocalization.Text("Menu.Solid"));
        AddCommands(solid, DemoCommandId.Box, DemoCommandId.Cylinder, DemoCommandId.Frustum, DemoCommandId.Cone, DemoCommandId.Torus, DemoCommandId.Sphere, DemoCommandId.Wedge, DemoCommandId.Pipe);
        solid.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(solid, DemoCommandId.Extrude, DemoCommandId.Revolve, DemoCommandId.Sweep, DemoCommandId.Loft);
        solid.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(solid, DemoCommandId.Fuse, DemoCommandId.Cut, DemoCommandId.Common, DemoCommandId.Section);
        solid.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(solid, DemoCommandId.Fillet, DemoCommandId.Chamfer, DemoCommandId.Offset, DemoCommandId.Shell, DemoCommandId.Drill);

        var annotate = new ToolStripMenuItem(DemoLocalization.Text("Menu.Annotate"));
        AddCommands(annotate, DemoCommandId.Text);
        annotate.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(annotate, DemoCommandId.LengthDimension, DemoCommandId.AngleDimension, DemoCommandId.RadiusDimension, DemoCommandId.DiameterDimension);

        var tools = new ToolStripMenuItem(DemoLocalization.Text("Menu.Tools"));
        AddCommands(tools, DemoCommandId.AnalyzeBounds, DemoCommandId.AnalyzeMass, DemoCommandId.AnalyzeTopology, DemoCommandId.AnalyzeDistance, DemoCommandId.ValidateShape);

        var samples = new ToolStripMenuItem(DemoLocalization.Text("Menu.Samples"));
        AddCommands(samples, DemoCommandId.DemoElements, DemoCommandId.DemoGear, DemoCommandId.DemoManifold, DemoCommandId.DemoTwistedDuct);
        samples.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(samples, DemoCommandId.DemoBracket, DemoCommandId.DemoFlange, DemoCommandId.DemoAnnotations);

        var language = new ToolStripMenuItem(DemoLocalization.Text("Menu.Language"));
        var english = new ToolStripMenuItem(DemoLocalization.Text("Menu.English")) { Checked = DemoLocalization.CurrentLanguage == DemoLanguage.English };
        var chinese = new ToolStripMenuItem(DemoLocalization.Text("Menu.Chinese")) { Checked = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified };
        english.Click += (_, _) => SetLanguage(DemoLanguage.English);
        chinese.Click += (_, _) => SetLanguage(DemoLanguage.ChineseSimplified);
        language.DropDownItems.Add(english);
        language.DropDownItems.Add(chinese);

        var help = new ToolStripMenuItem(DemoLocalization.Text("Menu.Help"));
        help.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.MouseHelp"), (_, _) => ShowMouseHelp()));
        help.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.About"), (_, _) => ShowAbout()));

        _menu.Items.AddRange(new ToolStripItem[] { file, edit, draw, solid, annotate, BuildViewMenu(), tools, samples, language, help });
        UpdateHistoryUi();
    }

    private ToolStripMenuItem BuildViewMenu()
    {
        var view = new ToolStripMenuItem(DemoLocalization.Text("Menu.View"));

        // Standard views (flattened)
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Front"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Front), "1"));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Back"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Back)));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Left"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Left), "2"));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Right"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Right)));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Top"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Top), "3"));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Bottom"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Bottom)));
        view.DropDownItems.Add(new ToolStripSeparator());
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric), "0"));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.NorthEast"), (_, _) => Session.SetIsoView(DemoIsoView.NorthEast)));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.NorthWest"), (_, _) => Session.SetIsoView(DemoIsoView.NorthWest)));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.SouthEast"), (_, _) => Session.SetIsoView(DemoIsoView.SouthEast)));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.SouthWest"), (_, _) => Session.SetIsoView(DemoIsoView.SouthWest)));
        view.DropDownItems.Add(new ToolStripSeparator());

        // Display style (flattened)
        view.DropDownItems.Add(RadioMenuItem(DemoLocalization.Text("Menu.Shaded"), _displayMode == OcctDisplayMode.Shaded, (_, _) => SetDisplayStyle(OcctDisplayMode.Shaded)));
        view.DropDownItems.Add(RadioMenuItem(DemoLocalization.Text("Menu.Wireframe"), _displayMode == OcctDisplayMode.Wireframe, (_, _) => SetDisplayStyle(OcctDisplayMode.Wireframe)));
        view.DropDownItems.Add(CheckMenuItem(DemoLocalization.Text("Menu.ShadedEdges"), true, (_, item) => ExecuteSafe(() => Session.Engine.SetFaceBoundariesVisible(item.Checked))));
        view.DropDownItems.Add(new ToolStripSeparator());

        // Everything else lives in the non-modal View Settings window.
        view.DropDownItems.Add(MenuItem(Local("View Settings...", "视图设置..."), (_, _) => ShowAdvancedViewSettingsWindow()));
        return view;
    }

    private static ToolStripMenuItem RadioMenuItem(string text, bool checkedState, EventHandler click)
    {
        var item = new ToolStripMenuItem(text) { Checked = checkedState };
        item.Click += click;
        return item;
    }

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
        _toolBar.Items.Add(CommandButton(DemoLocalization.Text("Toolbar.Line"), DemoCommandId.Line));
        _toolBar.Items.Add(CommandButton(DemoLocalization.Text("Toolbar.Circle"), DemoCommandId.Circle));
        _toolBar.Items.Add(CommandButton(DemoLocalization.Text("Toolbar.Box"), DemoCommandId.Box));
        _toolBar.Items.Add(CommandButton(DemoLocalization.Text("Toolbar.Cylinder"), DemoCommandId.Cylinder));
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Extents"), (_, _) => Session.Engine.FitAll()));
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric)));
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(new ToolStripLabel(DemoLocalization.Text("Toolbar.Selection")));
        foreach (var mode in Enum.GetValues<OcctSelectionMode>()) _selectionCombo.Items.Add(SelectionModeName(mode));
        _selectionCombo.SelectedIndex = Math.Min(selectedIndex, _selectionCombo.Items.Count - 1);
        _toolBar.Items.Add(_selectionCombo);
        UpdateHistoryUi();
    }

    private void AddCommands(ToolStripMenuItem parent, params DemoCommandId[] commandIds)
    {
        foreach (var id in commandIds)
        {
            var definition = DemoLocalization.Localize(DemoCommandCatalog.Get(id));
            var item = new ToolStripMenuItem(definition.Text) { Tag = id, ToolTipText = definition.Description };
            if (!string.IsNullOrWhiteSpace(definition.Shortcut)) item.ShortcutKeyDisplayString = definition.Shortcut;
            item.Click += (_, _) => RunCommand((DemoCommandId)item.Tag!);
            parent.DropDownItems.Add(item);
        }
    }
}
