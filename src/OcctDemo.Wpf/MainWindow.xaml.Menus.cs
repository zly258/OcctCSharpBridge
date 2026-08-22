using System.Globalization;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using Controls = System.Windows.Controls;
using Input = System.Windows.Input;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private void BuildMenus()
    {
        MainMenu.Items.Clear();

        var file = Menu(MenuHeader("Menu.File"));
        file.Items.Add(MenuItem(DemoLocalization.Text("Menu.New"), (_, _) => NewDocument(), "Ctrl+N"));
        file.Items.Add(MenuItem(DemoLocalization.Text("Menu.Open"), (_, _) => OpenDocument(), "Ctrl+O"));
        file.Items.Add(MenuItem(DemoLocalization.Text("Menu.Save"), (_, _) => SaveDocument(false), "Ctrl+S"));
        file.Items.Add(MenuItem(DemoLocalization.Text("Menu.SaveAs"), (_, _) => SaveDocument(true), "Ctrl+Shift+S"));
        file.Items.Add(new Controls.Separator());
        file.Items.Add(MenuItem(DemoLocalization.Text("Menu.Import"), (_, _) => ImportDocument()));
        file.Items.Add(MenuItem(DemoLocalization.Text("Menu.ExportSelected"), (_, _) => ExportSelected()));
        file.Items.Add(MenuItem(DemoLocalization.Text("Menu.ExportImage"), (_, _) => ExportViewImage()));
        file.Items.Add(new Controls.Separator());
        file.Items.Add(MenuItem(DemoLocalization.Text("Menu.Exit"), (_, _) => Close(), "Alt+F4"));

        var edit = Menu(MenuHeader("Menu.Edit"));
        _undoMenuItem = MenuItem(DemoLocalization.Text("Menu.Undo"), (_, _) => Undo(), "Ctrl+Z");
        _redoMenuItem = MenuItem(DemoLocalization.Text("Menu.Redo"), (_, _) => Redo(), "Ctrl+Y");
        edit.Items.Add(_undoMenuItem);
        edit.Items.Add(_redoMenuItem);
        edit.Items.Add(new Controls.Separator());
        AddCommands(edit, DemoCommandId.Translate, DemoCommandId.Rotate, DemoCommandId.Scale, DemoCommandId.Mirror, DemoCommandId.Copy);
        edit.Items.Add(new Controls.Separator());
        AddCommands(edit, DemoCommandId.Delete);
        edit.Items.Add(MenuItem(DemoLocalization.Text("Menu.ClearSelection"), (_, _) =>
        {
            Session.Engine.ClearSelection();
            Viewport.RaiseSelectionChanged();
        }));
        edit.Items.Add(MenuItem(DemoLocalization.Text("Menu.ShowAll"), (_, _) => Session.Engine.ShowAll()));
        edit.Items.Add(MenuItem(DemoLocalization.Text("Menu.HideAll"), (_, _) => Session.Engine.HideAll()));

        var draw = Menu(MenuHeader("Menu.Draw"));
        AddCommands(draw, DemoCommandId.Point, DemoCommandId.Line, DemoCommandId.Polyline);
        draw.Items.Add(new Controls.Separator());
        AddCommands(draw, DemoCommandId.Circle, DemoCommandId.ArcThreePoints, DemoCommandId.ArcCenter, DemoCommandId.Ellipse);
        draw.Items.Add(new Controls.Separator());
        AddCommands(draw, DemoCommandId.Rectangle, DemoCommandId.Polygon, DemoCommandId.Bezier, DemoCommandId.BSpline);

        var solid = Menu(MenuHeader("Menu.Solid"));
        AddCommands(solid, DemoCommandId.Box, DemoCommandId.Cylinder, DemoCommandId.Frustum, DemoCommandId.Cone,
            DemoCommandId.Torus, DemoCommandId.Sphere, DemoCommandId.Wedge, DemoCommandId.Pipe);
        solid.Items.Add(new Controls.Separator());
        AddCommands(solid, DemoCommandId.Extrude, DemoCommandId.Revolve, DemoCommandId.Sweep, DemoCommandId.Loft);
        solid.Items.Add(new Controls.Separator());
        AddCommands(solid, DemoCommandId.Fuse, DemoCommandId.Cut, DemoCommandId.Common, DemoCommandId.Section);
        solid.Items.Add(new Controls.Separator());
        AddCommands(solid, DemoCommandId.Fillet, DemoCommandId.Chamfer, DemoCommandId.Offset, DemoCommandId.Shell, DemoCommandId.Drill);

        var annotate = Menu(MenuHeader("Menu.Annotate"));
        AddCommands(annotate, DemoCommandId.Text);
        annotate.Items.Add(new Controls.Separator());
        AddCommands(annotate, DemoCommandId.LengthDimension, DemoCommandId.AngleDimension,
            DemoCommandId.RadiusDimension, DemoCommandId.DiameterDimension);
        annotate.Items.Add(new Controls.Separator());
        AddCommands(annotate, DemoCommandId.DemoAnnotations);

        var tools = Menu(MenuHeader("Menu.Tools"));
        AddCommands(tools, DemoCommandId.AnalyzeBounds, DemoCommandId.AnalyzeMass, DemoCommandId.AnalyzeTopology,
            DemoCommandId.AnalyzeDistance, DemoCommandId.ValidateShape);

        var samples = Menu(MenuHeader("Menu.Samples"));
        AddCommands(samples, DemoCommandId.DemoElements, DemoCommandId.DemoGear, DemoCommandId.DemoManifold,
            DemoCommandId.DemoTwistedDuct);
        samples.Items.Add(new Controls.Separator());
        AddCommands(samples, DemoCommandId.DemoBracket, DemoCommandId.DemoFlange);
        samples.Items.Add(new Controls.Separator());
        AddCommands(samples, DemoCommandId.DemoAnnotations);

        var language = Menu(MenuHeader("Menu.Language"));
        var english = MenuItem(DemoLocalization.Text("Menu.English"), (_, _) => SetLanguage(DemoLanguage.English));
        var chinese = MenuItem(DemoLocalization.Text("Menu.Chinese"), (_, _) => SetLanguage(DemoLanguage.ChineseSimplified));
        english.IsCheckable = true;
        english.IsChecked = DemoLocalization.CurrentLanguage == DemoLanguage.English;
        chinese.IsCheckable = true;
        chinese.IsChecked = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified;
        language.Items.Add(english);
        language.Items.Add(chinese);

        var help = Menu(MenuHeader("Menu.Help"));
        help.Items.Add(MenuItem(DemoLocalization.Text("Menu.MouseHelp"), (_, _) => ShowMouseHelp()));
        help.Items.Add(MenuItem(DemoLocalization.Text("Menu.About"), (_, _) => ShowAbout()));

        MainMenu.Items.Add(file);
        MainMenu.Items.Add(edit);
        MainMenu.Items.Add(draw);
        MainMenu.Items.Add(solid);
        MainMenu.Items.Add(annotate);
        MainMenu.Items.Add(BuildViewMenu());
        MainMenu.Items.Add(tools);
        MainMenu.Items.Add(samples);
        MainMenu.Items.Add(language);
        MainMenu.Items.Add(help);
        UpdateHistoryUi();
    }

    private Controls.MenuItem BuildViewMenu()
    {
        var view = Menu(MenuHeader("Menu.View"));

        // Standard views (flattened)
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.Front"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Front), "1"));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.Back"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Back)));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.Left"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Left), "2"));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.Right"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Right)));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.Top"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Top), "3"));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.Bottom"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Bottom)));
        view.Items.Add(new Controls.Separator());
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric), "0"));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.NorthEast"), (_, _) => Session.SetIsoView(DemoIsoView.NorthEast)));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.NorthWest"), (_, _) => Session.SetIsoView(DemoIsoView.NorthWest)));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.SouthEast"), (_, _) => Session.SetIsoView(DemoIsoView.SouthEast)));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.SouthWest"), (_, _) => Session.SetIsoView(DemoIsoView.SouthWest)));
        view.Items.Add(new Controls.Separator());

        // Display style (flattened)
        view.Items.Add(RadioMenuItem(DemoLocalization.Text("Menu.Shaded"), _displayMode == OcctDisplayMode.Shaded, () => SetDisplayStyle(OcctDisplayMode.Shaded)));
        view.Items.Add(RadioMenuItem(DemoLocalization.Text("Menu.Wireframe"), _displayMode == OcctDisplayMode.Wireframe, () => SetDisplayStyle(OcctDisplayMode.Wireframe)));
        view.Items.Add(CheckMenuItem(DemoLocalization.Text("Menu.ShadedEdges"), true, item => ExecuteSafe(() => Session.Engine.SetFaceBoundariesVisible(item.IsChecked))));
        view.Items.Add(new Controls.Separator());

        // Everything else lives in the non-modal View Settings window.
        view.Items.Add(MenuItem(Local("View Settings...", "视图设置..."), (_, _) => ShowAdvancedViewSettingsWindow()));
        return view;
    }

    private static Controls.MenuItem RadioMenuItem(string text, bool isChecked, Action apply)
    {
        var item = new Controls.MenuItem { Header = text, IsCheckable = true, IsChecked = isChecked };
        item.Click += (_, _) => apply();
        return item;
    }

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
        MainToolBar.Items.Add(CommandButton(DemoLocalization.Text("Toolbar.Line"), DemoCommandId.Line));
        MainToolBar.Items.Add(CommandButton(DemoLocalization.Text("Toolbar.Circle"), DemoCommandId.Circle));
        MainToolBar.Items.Add(CommandButton(DemoLocalization.Text("Toolbar.Box"), DemoCommandId.Box));
        MainToolBar.Items.Add(CommandButton(DemoLocalization.Text("Toolbar.Cylinder"), DemoCommandId.Cylinder));
        MainToolBar.Items.Add(new Controls.Separator());
        MainToolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        MainToolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        MainToolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Extents"), (_, _) => Session.Engine.FitAll()));
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

    private void AddCommands(Controls.MenuItem parent, params DemoCommandId[] commands)
    {
        foreach (var id in commands)
        {
            var definition = DemoLocalization.Localize(DemoCommandCatalog.Get(id));
            var item = MenuItem(definition.Text, (_, _) => RunCommand(id), definition.Shortcut);
            item.ToolTip = definition.Description;
            parent.Items.Add(item);
        }
    }
}
