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
        var primitives = Menu(MenuHeader("Menu.Primitives"));
        AddCommands(primitives, DemoCommandId.Box, DemoCommandId.Cylinder, DemoCommandId.Frustum, DemoCommandId.Cone,
            DemoCommandId.Torus, DemoCommandId.Sphere, DemoCommandId.Wedge, DemoCommandId.Pipe);
        var features = Menu(MenuHeader("Menu.Features"));
        AddCommands(features, DemoCommandId.Extrude, DemoCommandId.Revolve, DemoCommandId.Sweep, DemoCommandId.Loft);
        var booleans = Menu(MenuHeader("Menu.Boolean"));
        AddCommands(booleans, DemoCommandId.Fuse, DemoCommandId.Cut, DemoCommandId.Common, DemoCommandId.Section);
        var details = Menu(MenuHeader("Menu.Details"));
        AddCommands(details, DemoCommandId.Fillet, DemoCommandId.Chamfer, DemoCommandId.Offset, DemoCommandId.Shell, DemoCommandId.Drill);
        solid.Items.Add(primitives);
        solid.Items.Add(features);
        solid.Items.Add(booleans);
        solid.Items.Add(details);

        var annotate = Menu(MenuHeader("Menu.Annotate"));
        AddCommands(annotate, DemoCommandId.Text);
        annotate.Items.Add(new Controls.Separator());
        AddCommands(annotate, DemoCommandId.LengthDimension, DemoCommandId.AngleDimension,
            DemoCommandId.RadiusDimension, DemoCommandId.DiameterDimension);

        var tools = Menu(MenuHeader("Menu.Tools"));
        AddCommands(tools, DemoCommandId.AnalyzeBounds, DemoCommandId.AnalyzeMass, DemoCommandId.AnalyzeTopology,
            DemoCommandId.AnalyzeDistance, DemoCommandId.ValidateShape);

        var samples = Menu(MenuHeader("Menu.Samples"));
        AddCommands(samples, DemoCommandId.DemoElements, DemoCommandId.DemoGear, DemoCommandId.DemoManifold,
            DemoCommandId.DemoTwistedDuct);
        samples.Items.Add(new Controls.Separator());
        AddCommands(samples, DemoCommandId.DemoBracket, DemoCommandId.DemoFlange, DemoCommandId.DemoAnnotations);

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
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.FitAll"), (_, _) => Session.Engine.FitAll(), "F"));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.FitSelected"), (_, _) =>
        {
            var shape = ActiveShape();
            if (shape is not null) Session.Engine.Fit(shape.Value);
        }));
        view.Items.Add(new Controls.Separator());

        var display = Menu(MenuHeader("Menu.Display"));
        display.Items.Add(MenuItem(DemoLocalization.Text("Menu.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        display.Items.Add(CheckMenuItem(DemoLocalization.Text("Menu.ShadedEdges"), true, item => ExecuteSafe(() => Session.Engine.SetFaceBoundariesVisible(item.IsChecked))));
        display.Items.Add(MenuItem(DemoLocalization.Text("Menu.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        display.Items.Add(CheckMenuItem(DemoLocalization.Text("Menu.Hlr"), false, item => Session.Engine.SetComputedHlr(item.IsChecked)));
        display.Items.Add(CheckMenuItem(DemoLocalization.Text("Menu.Antialiasing"), true, item => Session.Engine.SetAntialiasing(item.IsChecked)));
        display.Items.Add(CheckMenuItem(DemoLocalization.Text("Menu.Triedron"), true, item => ExecuteSafe(() => Session.Engine.SetTriedronVisible(item.IsChecked))));
        display.Items.Add(CheckMenuItem(DemoLocalization.Text("Menu.ViewCube"), true, item => ExecuteSafe(() => Session.Engine.SetViewCubeVisible(item.IsChecked))));
        view.Items.Add(display);
        view.Items.Add(BuildDepthMenu());

        var standard = Menu(MenuHeader("Menu.StandardViews"));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Front"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Front), "1"));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Back"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Back)));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Left"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Left), "2"));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Right"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Right)));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Top"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Top), "3"));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Bottom"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Bottom)));
        standard.Items.Add(new Controls.Separator());
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric), "0"));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.NorthEast"), (_, _) => Session.SetIsoView(DemoIsoView.NorthEast)));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.NorthWest"), (_, _) => Session.SetIsoView(DemoIsoView.NorthWest)));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.SouthEast"), (_, _) => Session.SetIsoView(DemoIsoView.SouthEast)));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.SouthWest"), (_, _) => Session.SetIsoView(DemoIsoView.SouthWest)));
        view.Items.Add(standard);

        var projection = Menu(MenuHeader("Menu.Projection"));
        projection.Items.Add(MenuItem(DemoLocalization.Text("Menu.Orthographic"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Orthographic)));
        projection.Items.Add(MenuItem(DemoLocalization.Text("Menu.Perspective"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Perspective)));
        projection.Items.Add(MenuItem(DemoLocalization.Text("Menu.PerspectiveFov"), (_, _) => SetPerspectiveFov()));
        view.Items.Add(projection);

        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.DisplayPrecision"), (_, _) => SetDisplayPrecision()));
        view.Items.Add(BuildLightingMenu());
        view.Items.Add(BuildMaterialMenu());
        view.Items.Add(BuildSelectionMenu());
        view.Items.Add(BuildSelectionAppearanceMenu());
        view.Items.Add(CheckMenuItem(DemoLocalization.Text("Menu.WindowSelection"), Viewport.EnableRectangleSelection, item =>
        {
            Viewport.EnableRectangleSelection = item.IsChecked;
            CommandStatus.Text = DemoLocalization.Text(item.IsChecked ? "Status.WindowSelectionOn" : "Status.WindowSelectionOff");
        }));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.SelectionTolerance"), (_, _) => SetSelectionTolerance()));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.Background"), (_, _) => SetBackgroundColor()));
        view.Items.Add(MenuItem(DemoLocalization.Text("Menu.GradientBackground"), (_, _) =>
            Session.Engine.SetGradientBackground(DrawingColor.White, DrawingColor.LightSteelBlue)));
        return view;
    }

    private Controls.MenuItem BuildDepthMenu()
    {
        var menu = Menu(MenuHeader("Menu.DepthHandling"));
        menu.Items.Add(CheckMenuItem(
            DemoLocalization.Text("Menu.AutoZFit"),
            _autoZFitEnabled,
            item => ExecuteSafe(() =>
            {
                _autoZFitEnabled = item.IsChecked;
                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                var message = DemoLocalization.Text(
                    _autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                CommandStatus.Text = message;
                Log(message);
            })));
        menu.Items.Add(MenuItem(
            DemoLocalization.Text("Menu.AutoZFitNow"),
            (_, _) => ExecuteSafe(Session.Engine.AutoZFit)));
        menu.Items.Add(new Controls.Separator());
        menu.Items.Add(MenuItem(
            DemoLocalization.Text("Menu.DepthForward"),
            (_, _) => ApplyDepthBias(DemoDepthBiasPreset.Forward)));
        menu.Items.Add(MenuItem(
            DemoLocalization.Text("Menu.DepthBackward"),
            (_, _) => ApplyDepthBias(DemoDepthBiasPreset.Backward)));
        menu.Items.Add(MenuItem(
            DemoLocalization.Text("Menu.DepthReset"),
            (_, _) => ApplyDepthBias(DemoDepthBiasPreset.Default)));
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
                    DemoLocalization.Text("Dialog.ApplyExistingMaterial"),
                    DemoLocalization.Text("Menu.Material"),
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes;
                Session.Engine.SetDefaultMaterial(captured, apply);
                Log($"{DemoLocalization.Text("Menu.Material")}: {MaterialDisplayName(captured)}");
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
