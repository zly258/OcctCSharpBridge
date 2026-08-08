using System.Globalization;
using CadCommon;
using OcctNet;

namespace CadWinForms;

public sealed partial class MainForm
{
    private void BuildMenus()
    {
        _menu.Items.Clear();

        var file = new ToolStripMenuItem(CadLocalization.Text("Menu.File"));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.New"), (_, _) => NewDocument(), "Ctrl+N"));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Open"), (_, _) => OpenDocument(), "Ctrl+O"));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Save"), (_, _) => SaveDocument(false), "Ctrl+S"));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.SaveAs"), (_, _) => SaveDocument(true), "Ctrl+Shift+S"));
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Import"), (_, _) => ImportDocument()));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.ExportSelected"), (_, _) => ExportSelected()));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.ExportImage"), (_, _) => ExportViewImage()));
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Exit"), (_, _) => Close(), "Alt+F4"));

        var edit = new ToolStripMenuItem(CadLocalization.Text("Menu.Edit"));
        _undoMenuItem = MenuItem(CadLocalization.Text("Menu.Undo"), (_, _) => Undo(), "Ctrl+Z");
        _redoMenuItem = MenuItem(CadLocalization.Text("Menu.Redo"), (_, _) => Redo(), "Ctrl+Y");
        edit.DropDownItems.Add(_undoMenuItem);
        edit.DropDownItems.Add(_redoMenuItem);
        edit.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(edit, CadCommandId.Translate, CadCommandId.Rotate, CadCommandId.Scale, CadCommandId.Mirror, CadCommandId.Copy);
        edit.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(edit, CadCommandId.Delete);
        edit.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.ClearSelection"), (_, _) => { Session.Engine.ClearSelection(); _viewport.RaiseSelectionChanged(); }));
        edit.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.ShowAll"), (_, _) => Session.Engine.ShowAll()));
        edit.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.HideAll"), (_, _) => Session.Engine.HideAll()));

        var draw = new ToolStripMenuItem(CadLocalization.Text("Menu.Draw"));
        AddCommands(draw, CadCommandId.Point, CadCommandId.Line, CadCommandId.Polyline);
        draw.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(draw, CadCommandId.Circle, CadCommandId.ArcThreePoints, CadCommandId.ArcCenter, CadCommandId.Ellipse);
        draw.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(draw, CadCommandId.Rectangle, CadCommandId.Polygon, CadCommandId.Bezier, CadCommandId.BSpline);

        var solid = new ToolStripMenuItem(CadLocalization.Text("Menu.Solid"));
        var primitives = new ToolStripMenuItem(CadLocalization.Text("Menu.Primitives"));
        AddCommands(primitives, CadCommandId.Box, CadCommandId.Cylinder, CadCommandId.Frustum, CadCommandId.Cone, CadCommandId.Torus, CadCommandId.Sphere, CadCommandId.Wedge, CadCommandId.Pipe);
        var features = new ToolStripMenuItem(CadLocalization.Text("Menu.Features"));
        AddCommands(features, CadCommandId.Extrude, CadCommandId.Revolve, CadCommandId.Sweep, CadCommandId.Loft);
        var booleans = new ToolStripMenuItem(CadLocalization.Text("Menu.Boolean"));
        AddCommands(booleans, CadCommandId.Fuse, CadCommandId.Cut, CadCommandId.Common, CadCommandId.Section);
        var details = new ToolStripMenuItem(CadLocalization.Text("Menu.Details"));
        AddCommands(details, CadCommandId.Fillet, CadCommandId.Chamfer, CadCommandId.Offset, CadCommandId.Shell, CadCommandId.Drill);
        solid.DropDownItems.AddRange(new ToolStripItem[] { primitives, features, booleans, details });

        var annotate = new ToolStripMenuItem(CadLocalization.Text("Menu.Annotate"));
        AddCommands(annotate, CadCommandId.Text);
        annotate.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(annotate, CadCommandId.LengthDimension, CadCommandId.AngleDimension, CadCommandId.RadiusDimension, CadCommandId.DiameterDimension);

        var tools = new ToolStripMenuItem(CadLocalization.Text("Menu.Tools"));
        AddCommands(tools, CadCommandId.AnalyzeBounds, CadCommandId.AnalyzeMass, CadCommandId.AnalyzeTopology, CadCommandId.AnalyzeDistance, CadCommandId.ValidateShape);

        var samples = new ToolStripMenuItem(CadLocalization.Text("Menu.Samples"));
        AddCommands(samples, CadCommandId.DemoElements, CadCommandId.DemoGear, CadCommandId.DemoManifold, CadCommandId.DemoTwistedDuct);
        samples.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(samples, CadCommandId.DemoBracket, CadCommandId.DemoFlange, CadCommandId.DemoAnnotations);

        var language = new ToolStripMenuItem(CadLocalization.Text("Menu.Language"));
        var english = new ToolStripMenuItem(CadLocalization.Text("Menu.English")) { Checked = CadLocalization.CurrentLanguage == CadLanguage.English };
        var chinese = new ToolStripMenuItem(CadLocalization.Text("Menu.Chinese")) { Checked = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified };
        english.Click += (_, _) => SetLanguage(CadLanguage.English);
        chinese.Click += (_, _) => SetLanguage(CadLanguage.ChineseSimplified);
        language.DropDownItems.Add(english);
        language.DropDownItems.Add(chinese);

        var help = new ToolStripMenuItem(CadLocalization.Text("Menu.Help"));
        help.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.MouseHelp"), (_, _) => ShowMouseHelp()));
        help.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.About"), (_, _) => ShowAbout()));

        _menu.Items.AddRange(new ToolStripItem[] { file, edit, draw, solid, annotate, BuildViewMenu(), tools, samples, language, help });
        UpdateHistoryUi();
    }

    private ToolStripMenuItem BuildViewMenu()
    {
        var view = new ToolStripMenuItem(CadLocalization.Text("Menu.View"));
        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.FitAll"), (_, _) => Session.Engine.FitAll(), "F"));
        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.FitSelected"), (_, _) => { var shape = ActiveShape(); if (shape is not null) Session.Engine.Fit(shape.Value); }));
        view.DropDownItems.Add(new ToolStripSeparator());

        var display = new ToolStripMenuItem(CadLocalization.Text("Menu.Display"));
        display.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        display.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.ShadedEdges"), true, (_, item) => ExecuteSafe(() => Session.Engine.SetFaceBoundariesVisible(item.Checked))));
        display.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        display.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.Hlr"), false, (_, item) => Session.Engine.SetComputedHlr(item.Checked)));
        display.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.Antialiasing"), true, (_, item) => Session.Engine.SetAntialiasing(item.Checked)));
        display.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.Triedron"), true, (_, item) => ExecuteSafe(() => Session.Engine.SetTriedronVisible(item.Checked))));
        display.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.ViewCube"), true, (_, item) => ExecuteSafe(() => Session.Engine.SetViewCubeVisible(item.Checked))));
        view.DropDownItems.Add(display);
        view.DropDownItems.Add(BuildDepthMenu());

        var standard = new ToolStripMenuItem(CadLocalization.Text("Menu.StandardViews"));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Front"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Front), "1"));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Back"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Back)));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Left"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Left), "2"));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Right"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Right)));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Top"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Top), "3"));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Bottom"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Bottom)));
        standard.DropDownItems.Add(new ToolStripSeparator());
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric), "0"));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.NorthEast"), (_, _) => Session.SetIsoView(CadIsoView.NorthEast)));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.NorthWest"), (_, _) => Session.SetIsoView(CadIsoView.NorthWest)));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.SouthEast"), (_, _) => Session.SetIsoView(CadIsoView.SouthEast)));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.SouthWest"), (_, _) => Session.SetIsoView(CadIsoView.SouthWest)));
        view.DropDownItems.Add(standard);

        var projection = new ToolStripMenuItem(CadLocalization.Text("Menu.Projection"));
        projection.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Orthographic"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Orthographic)));
        projection.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Perspective"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Perspective)));
        projection.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.PerspectiveFov"), (_, _) => SetPerspectiveFov()));
        view.DropDownItems.Add(projection);

        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.DisplayPrecision"), (_, _) => SetDisplayPrecision()));
        view.DropDownItems.Add(BuildLightingMenu());
        view.DropDownItems.Add(BuildMaterialMenu());
        view.DropDownItems.Add(BuildSelectionMenu());
        view.DropDownItems.Add(BuildSelectionAppearanceMenu());
        view.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.WindowSelection"), _viewport.EnableRectangleSelection, (_, item) =>
        {
            _viewport.EnableRectangleSelection = item.Checked;
            _commandStatus.Text = CadLocalization.Text(item.Checked ? "Status.WindowSelectionOn" : "Status.WindowSelectionOff");
        }));
        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.SelectionTolerance"), (_, _) => SetSelectionTolerance()));
        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Background"), (_, _) => SetBackgroundColor()));
        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.GradientBackground"), (_, _) => Session.Engine.SetGradientBackground(Color.White, Color.LightSteelBlue)));
        return view;
    }

    private ToolStripMenuItem BuildDepthMenu()
    {
        var menu = new ToolStripMenuItem(CadLocalization.Text("Menu.DepthHandling"));
        menu.DropDownItems.Add(CheckMenuItem(
            CadLocalization.Text("Menu.AutoZFit"),
            _autoZFitEnabled,
            (_, item) => ExecuteSafe(() =>
            {
                _autoZFitEnabled = item.Checked;
                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                var message = CadLocalization.Text(
                    _autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                _commandStatus.Text = message;
                Log(message);
            })));
        menu.DropDownItems.Add(MenuItem(
            CadLocalization.Text("Menu.AutoZFitNow"),
            (_, _) => ExecuteSafe(Session.Engine.AutoZFit)));
        menu.DropDownItems.Add(new ToolStripSeparator());
        menu.DropDownItems.Add(MenuItem(
            CadLocalization.Text("Menu.DepthForward"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Forward)));
        menu.DropDownItems.Add(MenuItem(
            CadLocalization.Text("Menu.DepthBackward"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Backward)));
        menu.DropDownItems.Add(MenuItem(
            CadLocalization.Text("Menu.DepthReset"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Default)));
        return menu;
    }

    private ToolStripMenuItem BuildMaterialMenu()
    {
        var menu = new ToolStripMenuItem(CadLocalization.Text("Menu.Material"));
        foreach (OcctMaterial material in Enum.GetValues<OcctMaterial>())
        {
            var item = new ToolStripMenuItem(MaterialDisplayName(material)) { Tag = material };
            item.Click += (_, _) =>
            {
                var apply = MessageBox.Show(this, CadLocalization.Text("Dialog.ApplyExistingMaterial"), CadLocalization.Text("Menu.Material"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                Session.Engine.SetDefaultMaterial((OcctMaterial)item.Tag!, apply);
                Log($"{CadLocalization.Text("Menu.Material")}: {item.Text}");
            };
            menu.DropDownItems.Add(item);
        }
        return menu;
    }

    private ToolStripMenuItem BuildSelectionMenu()
    {
        var menu = new ToolStripMenuItem(CadLocalization.Text("Menu.SelectionMode"));
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
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.New"), (_, _) => NewDocument()));
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Open"), (_, _) => OpenDocument()));
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Save"), (_, _) => SaveDocument(false)));
        _toolBar.Items.Add(new ToolStripSeparator());
        _undoButton = ToolButton(CadLocalization.Text("Toolbar.Undo"), (_, _) => Undo());
        _redoButton = ToolButton(CadLocalization.Text("Toolbar.Redo"), (_, _) => Redo());
        _toolBar.Items.Add(_undoButton);
        _toolBar.Items.Add(_redoButton);
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Line"), CadCommandId.Line));
        _toolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Circle"), CadCommandId.Circle));
        _toolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Box"), CadCommandId.Box));
        _toolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Cylinder"), CadCommandId.Cylinder));
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Extents"), (_, _) => Session.Engine.FitAll()));
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric)));
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(new ToolStripLabel(CadLocalization.Text("Toolbar.Selection")));
        foreach (var mode in Enum.GetValues<OcctSelectionMode>()) _selectionCombo.Items.Add(SelectionModeName(mode));
        _selectionCombo.SelectedIndex = Math.Min(selectedIndex, _selectionCombo.Items.Count - 1);
        _toolBar.Items.Add(_selectionCombo);
        UpdateHistoryUi();
    }

    private void AddCommands(ToolStripMenuItem parent, params CadCommandId[] commandIds)
    {
        foreach (var id in commandIds)
        {
            var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));
            var item = new ToolStripMenuItem(definition.Text) { Tag = id, ToolTipText = definition.Description };
            if (!string.IsNullOrWhiteSpace(definition.Shortcut)) item.ShortcutKeyDisplayString = definition.Shortcut;
            item.Click += (_, _) => RunCommand((CadCommandId)item.Tag!);
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
