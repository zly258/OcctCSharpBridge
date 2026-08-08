using System.Globalization;
using CadCommon;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using Controls = System.Windows.Controls;
using Input = System.Windows.Input;

namespace CadWpf;

public partial class MainWindow
{
    private void BuildMenus()
    {
        MainMenu.Items.Clear();

        var file = Menu(MenuHeader("Menu.File"));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.New"), (_, _) => NewDocument(), "Ctrl+N"));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.Open"), (_, _) => OpenDocument(), "Ctrl+O"));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.Save"), (_, _) => SaveDocument(false), "Ctrl+S"));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.SaveAs"), (_, _) => SaveDocument(true), "Ctrl+Shift+S"));
        file.Items.Add(new Controls.Separator());
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.Import"), (_, _) => ImportDocument()));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.ExportSelected"), (_, _) => ExportSelected()));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.ExportImage"), (_, _) => ExportViewImage()));
        file.Items.Add(new Controls.Separator());
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.Exit"), (_, _) => Close(), "Alt+F4"));

        var edit = Menu(MenuHeader("Menu.Edit"));
        _undoMenuItem = MenuItem(CadLocalization.Text("Menu.Undo"), (_, _) => Undo(), "Ctrl+Z");
        _redoMenuItem = MenuItem(CadLocalization.Text("Menu.Redo"), (_, _) => Redo(), "Ctrl+Y");
        edit.Items.Add(_undoMenuItem);
        edit.Items.Add(_redoMenuItem);
        edit.Items.Add(new Controls.Separator());
        AddCommands(edit, CadCommandId.Translate, CadCommandId.Rotate, CadCommandId.Scale, CadCommandId.Mirror, CadCommandId.Copy);
        edit.Items.Add(new Controls.Separator());
        AddCommands(edit, CadCommandId.Delete);
        edit.Items.Add(MenuItem(CadLocalization.Text("Menu.ClearSelection"), (_, _) =>
        {
            Session.Engine.ClearSelection();
            Viewport.RaiseSelectionChanged();
        }));
        edit.Items.Add(MenuItem(CadLocalization.Text("Menu.ShowAll"), (_, _) => Session.Engine.ShowAll()));
        edit.Items.Add(MenuItem(CadLocalization.Text("Menu.HideAll"), (_, _) => Session.Engine.HideAll()));

        var draw = Menu(MenuHeader("Menu.Draw"));
        AddCommands(draw, CadCommandId.Point, CadCommandId.Line, CadCommandId.Polyline);
        draw.Items.Add(new Controls.Separator());
        AddCommands(draw, CadCommandId.Circle, CadCommandId.ArcThreePoints, CadCommandId.ArcCenter, CadCommandId.Ellipse);
        draw.Items.Add(new Controls.Separator());
        AddCommands(draw, CadCommandId.Rectangle, CadCommandId.Polygon, CadCommandId.Bezier, CadCommandId.BSpline);

        var solid = Menu(MenuHeader("Menu.Solid"));
        var primitives = Menu(MenuHeader("Menu.Primitives"));
        AddCommands(primitives, CadCommandId.Box, CadCommandId.Cylinder, CadCommandId.Frustum, CadCommandId.Cone,
            CadCommandId.Torus, CadCommandId.Sphere, CadCommandId.Wedge, CadCommandId.Pipe);
        var features = Menu(MenuHeader("Menu.Features"));
        AddCommands(features, CadCommandId.Extrude, CadCommandId.Revolve, CadCommandId.Sweep, CadCommandId.Loft);
        var booleans = Menu(MenuHeader("Menu.Boolean"));
        AddCommands(booleans, CadCommandId.Fuse, CadCommandId.Cut, CadCommandId.Common, CadCommandId.Section);
        var details = Menu(MenuHeader("Menu.Details"));
        AddCommands(details, CadCommandId.Fillet, CadCommandId.Chamfer, CadCommandId.Offset, CadCommandId.Shell, CadCommandId.Drill);
        solid.Items.Add(primitives);
        solid.Items.Add(features);
        solid.Items.Add(booleans);
        solid.Items.Add(details);

        var annotate = Menu(MenuHeader("Menu.Annotate"));
        AddCommands(annotate, CadCommandId.Text);
        annotate.Items.Add(new Controls.Separator());
        AddCommands(annotate, CadCommandId.LengthDimension, CadCommandId.AngleDimension,
            CadCommandId.RadiusDimension, CadCommandId.DiameterDimension);

        var tools = Menu(MenuHeader("Menu.Tools"));
        AddCommands(tools, CadCommandId.AnalyzeBounds, CadCommandId.AnalyzeMass, CadCommandId.AnalyzeTopology,
            CadCommandId.AnalyzeDistance, CadCommandId.ValidateShape);

        var samples = Menu(MenuHeader("Menu.Samples"));
        AddCommands(samples, CadCommandId.DemoElements, CadCommandId.DemoGear, CadCommandId.DemoManifold,
            CadCommandId.DemoTwistedDuct);
        samples.Items.Add(new Controls.Separator());
        AddCommands(samples, CadCommandId.DemoBracket, CadCommandId.DemoFlange, CadCommandId.DemoAnnotations);

        var language = Menu(MenuHeader("Menu.Language"));
        var english = MenuItem(CadLocalization.Text("Menu.English"), (_, _) => SetLanguage(CadLanguage.English));
        var chinese = MenuItem(CadLocalization.Text("Menu.Chinese"), (_, _) => SetLanguage(CadLanguage.ChineseSimplified));
        english.IsCheckable = true;
        english.IsChecked = CadLocalization.CurrentLanguage == CadLanguage.English;
        chinese.IsCheckable = true;
        chinese.IsChecked = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified;
        language.Items.Add(english);
        language.Items.Add(chinese);

        var help = Menu(MenuHeader("Menu.Help"));
        help.Items.Add(MenuItem(CadLocalization.Text("Menu.MouseHelp"), (_, _) => ShowMouseHelp()));
        help.Items.Add(MenuItem(CadLocalization.Text("Menu.About"), (_, _) => ShowAbout()));

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
        view.Items.Add(MenuItem(CadLocalization.Text("Menu.FitAll"), (_, _) => Session.Engine.FitAll(), "F"));
        view.Items.Add(MenuItem(CadLocalization.Text("Menu.FitSelected"), (_, _) =>
        {
            var shape = ActiveShape();
            if (shape is not null) Session.Engine.Fit(shape.Value);
        }));
        view.Items.Add(new Controls.Separator());

        var display = Menu(MenuHeader("Menu.Display"));
        display.Items.Add(MenuItem(CadLocalization.Text("Menu.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        display.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.ShadedEdges"), true, item => ExecuteSafe(() => Session.Engine.SetFaceBoundariesVisible(item.IsChecked))));
        display.Items.Add(MenuItem(CadLocalization.Text("Menu.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        display.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.Hlr"), false, item => Session.Engine.SetComputedHlr(item.IsChecked)));
        display.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.Antialiasing"), true, item => Session.Engine.SetAntialiasing(item.IsChecked)));
        display.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.Triedron"), true, item => ExecuteSafe(() => Session.Engine.SetTriedronVisible(item.IsChecked))));
        display.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.ViewCube"), true, item => ExecuteSafe(() => Session.Engine.SetViewCubeVisible(item.IsChecked))));
        view.Items.Add(display);
        view.Items.Add(BuildDepthMenu());

        var standard = Menu(MenuHeader("Menu.StandardViews"));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Front"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Front), "1"));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Back"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Back)));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Left"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Left), "2"));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Right"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Right)));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Top"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Top), "3"));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Bottom"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Bottom)));
        standard.Items.Add(new Controls.Separator());
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric), "0"));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.NorthEast"), (_, _) => Session.SetIsoView(CadIsoView.NorthEast)));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.NorthWest"), (_, _) => Session.SetIsoView(CadIsoView.NorthWest)));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.SouthEast"), (_, _) => Session.SetIsoView(CadIsoView.SouthEast)));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.SouthWest"), (_, _) => Session.SetIsoView(CadIsoView.SouthWest)));
        view.Items.Add(standard);

        var projection = Menu(MenuHeader("Menu.Projection"));
        projection.Items.Add(MenuItem(CadLocalization.Text("Menu.Orthographic"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Orthographic)));
        projection.Items.Add(MenuItem(CadLocalization.Text("Menu.Perspective"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Perspective)));
        projection.Items.Add(MenuItem(CadLocalization.Text("Menu.PerspectiveFov"), (_, _) => SetPerspectiveFov()));
        view.Items.Add(projection);

        view.Items.Add(MenuItem(CadLocalization.Text("Menu.DisplayPrecision"), (_, _) => SetDisplayPrecision()));
        view.Items.Add(BuildLightingMenu());
        view.Items.Add(BuildMaterialMenu());
        view.Items.Add(BuildSelectionMenu());
        view.Items.Add(BuildSelectionAppearanceMenu());
        view.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.WindowSelection"), Viewport.EnableRectangleSelection, item =>
        {
            Viewport.EnableRectangleSelection = item.IsChecked;
            CommandStatus.Text = CadLocalization.Text(item.IsChecked ? "Status.WindowSelectionOn" : "Status.WindowSelectionOff");
        }));
        view.Items.Add(MenuItem(CadLocalization.Text("Menu.SelectionTolerance"), (_, _) => SetSelectionTolerance()));
        view.Items.Add(MenuItem(CadLocalization.Text("Menu.Background"), (_, _) => SetBackgroundColor()));
        view.Items.Add(MenuItem(CadLocalization.Text("Menu.GradientBackground"), (_, _) =>
            Session.Engine.SetGradientBackground(DrawingColor.White, DrawingColor.LightSteelBlue)));
        return view;
    }

    private Controls.MenuItem BuildDepthMenu()
    {
        var menu = Menu(MenuHeader("Menu.DepthHandling"));
        menu.Items.Add(CheckMenuItem(
            CadLocalization.Text("Menu.AutoZFit"),
            _autoZFitEnabled,
            item => ExecuteSafe(() =>
            {
                _autoZFitEnabled = item.IsChecked;
                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                var message = CadLocalization.Text(
                    _autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                CommandStatus.Text = message;
                Log(message);
            })));
        menu.Items.Add(MenuItem(
            CadLocalization.Text("Menu.AutoZFitNow"),
            (_, _) => ExecuteSafe(Session.Engine.AutoZFit)));
        menu.Items.Add(new Controls.Separator());
        menu.Items.Add(MenuItem(
            CadLocalization.Text("Menu.DepthForward"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Forward)));
        menu.Items.Add(MenuItem(
            CadLocalization.Text("Menu.DepthBackward"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Backward)));
        menu.Items.Add(MenuItem(
            CadLocalization.Text("Menu.DepthReset"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Default)));
        return menu;
    }

    private Controls.MenuItem BuildMaterialMenu()
    {
        var menu = Menu(MenuHeader("Menu.Material"));
        foreach (var material in Enum.GetValues<OcctMaterial>())
        {
            var captured = material;
            var item = MenuItem(MaterialDisplayName(captured), (_, _) =>
            {
                var apply = System.Windows.MessageBox.Show(this,
                    CadLocalization.Text("Dialog.ApplyExistingMaterial"),
                    CadLocalization.Text("Menu.Material"),
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes;
                Session.Engine.SetDefaultMaterial(captured, apply);
                Log($"{CadLocalization.Text("Menu.Material")}: {MaterialDisplayName(captured)}");
            });
            menu.Items.Add(item);
        }
        return menu;
    }

    private Controls.MenuItem BuildSelectionMenu()
    {
        var menu = Menu(MenuHeader("Menu.SelectionMode"));
        foreach (var mode in Enum.GetValues<OcctSelectionMode>())
        {
            var captured = mode;
            menu.Items.Add(MenuItem(SelectionModeName(captured), (_, _) => SetSelectionMode(captured)));
        }
        return menu;
    }

    private void BuildToolbar()
    {
        var selectedIndex = _selectionCombo?.SelectedIndex ?? 0;
        MainToolBar.Items.Clear();
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.New"), (_, _) => NewDocument()));
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Open"), (_, _) => OpenDocument()));
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Save"), (_, _) => SaveDocument(false)));
        MainToolBar.Items.Add(new Controls.Separator());
        _undoButton = ToolButton(CadLocalization.Text("Toolbar.Undo"), (_, _) => Undo());
        _redoButton = ToolButton(CadLocalization.Text("Toolbar.Redo"), (_, _) => Redo());
        MainToolBar.Items.Add(_undoButton);
        MainToolBar.Items.Add(_redoButton);
        MainToolBar.Items.Add(new Controls.Separator());
        MainToolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Line"), CadCommandId.Line));
        MainToolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Circle"), CadCommandId.Circle));
        MainToolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Box"), CadCommandId.Box));
        MainToolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Cylinder"), CadCommandId.Cylinder));
        MainToolBar.Items.Add(new Controls.Separator());
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Extents"), (_, _) => Session.Engine.FitAll()));
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric)));
        MainToolBar.Items.Add(new Controls.Separator());
        MainToolBar.Items.Add(new Controls.TextBlock
        {
            Text = CadLocalization.Text("Toolbar.Selection"),
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

    private void AddCommands(Controls.MenuItem parent, params CadCommandId[] commands)
    {
        foreach (var id in commands)
        {
            var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));
            var item = MenuItem(definition.Text, (_, _) => RunCommand(id), definition.Shortcut);
            item.ToolTip = definition.Description;
            parent.Items.Add(item);
        }
    }

    private Controls.MenuItem BuildLightingMenu()
    {
        var menu = Menu(Local("Lighting", "灯光"));
        foreach (var preset in Enum.GetValues<OcctLightingPreset>())
        {
            var captured = preset;
            menu.Items.Add(MenuItem(LightingPresetName(captured), (_, _) => ApplyLightingPreset(captured)));
        }
        menu.Items.Add(new Controls.Separator());
        menu.Items.Add(MenuItem(Local("Custom Lighting...", "自定义灯光..."), (_, _) => SetAdvancedLighting()));
        menu.Items.Add(MenuItem(Local("OCCT Default Lights", "恢复 OCCT 默认灯光"), (_, _) => ExecuteSafe(Session.Engine.ResetSceneLighting)));
        return menu;
    }

    private Controls.MenuItem BuildSelectionAppearanceMenu()
    {
        var menu = Menu(Local("Selection Appearance", "选择外观"));
        menu.Items.Add(MenuItem(Local("Selected Color...", "选中高亮颜色..."), (_, _) => SetSelectionHighlightColor()));
        menu.Items.Add(MenuItem(Local("Hover Color...", "悬浮高亮颜色..."), (_, _) => SetHoverHighlightColor()));
        return menu;
    }
}
