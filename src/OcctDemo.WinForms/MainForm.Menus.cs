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
        var primitives = new ToolStripMenuItem(DemoLocalization.Text("Menu.Primitives"));
        AddCommands(primitives, DemoCommandId.Box, DemoCommandId.Cylinder, DemoCommandId.Frustum, DemoCommandId.Cone, DemoCommandId.Torus, DemoCommandId.Sphere, DemoCommandId.Wedge, DemoCommandId.Pipe);
        var features = new ToolStripMenuItem(DemoLocalization.Text("Menu.Features"));
        AddCommands(features, DemoCommandId.Extrude, DemoCommandId.Revolve, DemoCommandId.Sweep, DemoCommandId.Loft);
        var booleans = new ToolStripMenuItem(DemoLocalization.Text("Menu.Boolean"));
        AddCommands(booleans, DemoCommandId.Fuse, DemoCommandId.Cut, DemoCommandId.Common, DemoCommandId.Section);
        var details = new ToolStripMenuItem(DemoLocalization.Text("Menu.Details"));
        AddCommands(details, DemoCommandId.Fillet, DemoCommandId.Chamfer, DemoCommandId.Offset, DemoCommandId.Shell, DemoCommandId.Drill);
        solid.DropDownItems.AddRange(new ToolStripItem[] { primitives, features, booleans, details });

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
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.FitAll"), (_, _) => Session.Engine.FitAll(), "F"));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.FitSelected"), (_, _) => { var shape = ActiveShape(); if (shape is not null) Session.Engine.Fit(shape.Value); }));
        view.DropDownItems.Add(new ToolStripSeparator());

        var display = new ToolStripMenuItem(DemoLocalization.Text("Menu.Display"));
        display.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Shaded"), (_, _) => Session.SetSceneDisplayMode(OcctDisplayMode.Shaded)));
        display.DropDownItems.Add(CheckMenuItem(DemoLocalization.Text("Menu.ShadedEdges"), true, (_, item) => ExecuteSafe(() => Session.Engine.SetFaceBoundariesVisible(item.Checked))));
        display.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Wireframe"), (_, _) => Session.SetSceneDisplayMode(OcctDisplayMode.Wireframe)));
        display.DropDownItems.Add(CheckMenuItem(DemoLocalization.Text("Menu.Hlr"), false, (_, item) => Session.Engine.SetComputedHlr(item.Checked)));
        display.DropDownItems.Add(CheckMenuItem(DemoLocalization.Text("Menu.Antialiasing"), true, (_, item) => Session.Engine.SetAntialiasing(item.Checked)));
        display.DropDownItems.Add(CheckMenuItem(DemoLocalization.Text("Menu.Triedron"), true, (_, item) => ExecuteSafe(() => Session.Engine.SetTriedronVisible(item.Checked))));
        display.DropDownItems.Add(CheckMenuItem(DemoLocalization.Text("Menu.ViewCube"), true, (_, item) => ExecuteSafe(() => Session.Engine.SetViewCubeVisible(item.Checked))));
        view.DropDownItems.Add(display);
        view.DropDownItems.Add(BuildDepthMenu());

        var standard = new ToolStripMenuItem(DemoLocalization.Text("Menu.StandardViews"));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Front"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Front), "1"));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Back"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Back)));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Left"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Left), "2"));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Right"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Right)));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Top"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Top), "3"));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Bottom"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Bottom)));
        standard.DropDownItems.Add(new ToolStripSeparator());
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric), "0"));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.NorthEast"), (_, _) => Session.SetIsoView(DemoIsoView.NorthEast)));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.NorthWest"), (_, _) => Session.SetIsoView(DemoIsoView.NorthWest)));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.SouthEast"), (_, _) => Session.SetIsoView(DemoIsoView.SouthEast)));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.SouthWest"), (_, _) => Session.SetIsoView(DemoIsoView.SouthWest)));
        view.DropDownItems.Add(standard);

        var projection = new ToolStripMenuItem(DemoLocalization.Text("Menu.Projection"));
        projection.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Orthographic"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Orthographic)));
        projection.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Perspective"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Perspective)));
        projection.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.PerspectiveFov"), (_, _) => SetPerspectiveFov()));
        view.DropDownItems.Add(projection);

        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.DisplayPrecision"), (_, _) => SetDisplayPrecision()));
        view.DropDownItems.Add(BuildLightingMenu());
        view.DropDownItems.Add(BuildMaterialMenu());
        view.DropDownItems.Add(BuildSelectionMenu());
        view.DropDownItems.Add(BuildSelectionAppearanceMenu());
        view.DropDownItems.Add(CheckMenuItem(DemoLocalization.Text("Menu.WindowSelection"), _viewport.EnableRectangleSelection, (_, item) =>
        {
            _viewport.EnableRectangleSelection = item.Checked;
            _commandStatus.Text = DemoLocalization.Text(item.Checked ? "Status.WindowSelectionOn" : "Status.WindowSelectionOff");
        }));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.SelectionTolerance"), (_, _) => SetSelectionTolerance()));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Background"), (_, _) => SetBackgroundColor()));
        view.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.GradientBackground"), (_, _) => Session.Engine.SetGradientBackground(Color.White, Color.LightSteelBlue)));
        return view;
    }

    private ToolStripMenuItem BuildDepthMenu()
    {
        var menu = new ToolStripMenuItem(DemoLocalization.Text("Menu.DepthHandling"));
        menu.DropDownItems.Add(CheckMenuItem(
            DemoLocalization.Text("Menu.AutoZFit"),
            _autoZFitEnabled,
            (_, item) => ExecuteSafe(() =>
            {
                _autoZFitEnabled = item.Checked;
                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                var message = DemoLocalization.Text(
                    _autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                _commandStatus.Text = message;
                Log(message);
            })));
        menu.DropDownItems.Add(MenuItem(
            DemoLocalization.Text("Menu.AutoZFitNow"),
            (_, _) => ExecuteSafe(Session.Engine.AutoZFit)));
        menu.DropDownItems.Add(new ToolStripSeparator());
        menu.DropDownItems.Add(MenuItem(
            DemoLocalization.Text("Menu.DepthForward"),
            (_, _) => ApplyDepthBias(DemoDepthBiasPreset.Forward)));
        menu.DropDownItems.Add(MenuItem(
            DemoLocalization.Text("Menu.DepthBackward"),
            (_, _) => ApplyDepthBias(DemoDepthBiasPreset.Backward)));
        menu.DropDownItems.Add(MenuItem(
            DemoLocalization.Text("Menu.DepthReset"),
            (_, _) => ApplyDepthBias(DemoDepthBiasPreset.Default)));
        return menu;
    }

    private ToolStripMenuItem BuildMaterialMenu()
    {
        var menu = new ToolStripMenuItem(DemoLocalization.Text("Menu.Material"));
        foreach (OcctMaterial material in Enum.GetValues<OcctMaterial>())
        {
            var item = new ToolStripMenuItem(MaterialDisplayName(material)) { Tag = material };
            item.Click += (_, _) =>
            {
                var apply = MessageBox.Show(this, DemoLocalization.Text("Dialog.ApplyExistingMaterial"), DemoLocalization.Text("Menu.Material"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                Session.Engine.SetDefaultMaterial((OcctMaterial)item.Tag!, apply);
                Log($"{DemoLocalization.Text("Menu.Material")}: {item.Text}");
            };
            menu.DropDownItems.Add(item);
        }
        return menu;
    }

    private ToolStripMenuItem BuildSelectionMenu()
    {
        var menu = new ToolStripMenuItem(DemoLocalization.Text("Menu.SelectionMode"));
        foreach (OcctSelectionMode mode in Enum.GetValues<OcctSelectionMode>())
        {
            var item = new ToolStripMenuItem(SelectionModeName(mode)) { Tag = mode, CheckOnClick = true };
            item.Click += (_, _) => SetSelectionMode((OcctSelectionMode)item.Tag!);
            menu.DropDownItems.Add(item);
        }
        return menu;
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
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Shaded"), (_, _) => Session.SetSceneDisplayMode(OcctDisplayMode.Shaded)));
        _toolBar.Items.Add(ToolButton(DemoLocalization.Text("Toolbar.Wireframe"), (_, _) => Session.SetSceneDisplayMode(OcctDisplayMode.Wireframe)));
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

    private ToolStripMenuItem BuildLightingMenu()
    {
        var menu = new ToolStripMenuItem(Local("Lighting", "灯光"));
        foreach (var preset in Enum.GetValues<OcctLightingPreset>())
        {
            var captured = preset;
            menu.DropDownItems.Add(MenuItem(LightingPresetName(captured), (_, _) => ApplyLightingPreset(captured)));
        }
        menu.DropDownItems.Add(new ToolStripSeparator());
        menu.DropDownItems.Add(MenuItem(Local("Custom Lighting...", "自定义灯光..."), (_, _) => SetAdvancedLighting()));
        menu.DropDownItems.Add(MenuItem(Local("OCCT Default Lights", "恢复 OCCT 默认灯光"), (_, _) => ExecuteSafe(Session.Engine.ResetSceneLighting)));
        return menu;
    }

    private ToolStripMenuItem BuildSelectionAppearanceMenu()
    {
        var menu = new ToolStripMenuItem(Local("Selection Appearance", "选择外观"));
        menu.DropDownItems.Add(MenuItem(Local("Selected Color...", "选中高亮颜色..."), (_, _) => SetSelectionHighlightColor()));
        menu.DropDownItems.Add(MenuItem(Local("Hover Color...", "悬浮高亮颜色..."), (_, _) => SetHoverHighlightColor()));
        return menu;
    }
}
