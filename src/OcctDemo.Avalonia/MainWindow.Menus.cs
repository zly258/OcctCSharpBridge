using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using AvaloniaToolTip = Avalonia.Controls.ToolTip;
using MenuItem = Avalonia.Controls.MenuItem;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private void BuildMenus()
    {
        var file = Menu(MenuHeader("Menu.File"),
            AsyncMenuItem(DemoLocalization.Text("Menu.New"), NewDocumentAsync, Shortcut(Key.N, KeyModifiers.Control)),
            AsyncMenuItem(DemoLocalization.Text("Menu.Open"), OpenDocumentAsync, Shortcut(Key.O, KeyModifiers.Control)),
            AsyncMenuItem(DemoLocalization.Text("Menu.Save"), () => SaveDocumentAsync(false), Shortcut(Key.S, KeyModifiers.Control)),
            AsyncMenuItem(DemoLocalization.Text("Menu.SaveAs"), () => SaveDocumentAsync(true), Shortcut(Key.S, KeyModifiers.Control | KeyModifiers.Shift)),
            new Separator(),
            AsyncMenuItem(DemoLocalization.Text("Menu.Import"), ImportDocumentAsync),
            AsyncMenuItem(DemoLocalization.Text("Menu.ExportSelected"), ExportSelectedAsync),
            AsyncMenuItem(DemoLocalization.Text("Menu.ExportImage"), ExportViewImageAsync),
            new Separator(),
            MenuItem(DemoLocalization.Text("Menu.Exit"), Close));

        _undoMenuItem = MenuItem(DemoLocalization.Text("Menu.Undo"), Undo, Shortcut(Key.Z, KeyModifiers.Control));
        _redoMenuItem = MenuItem(DemoLocalization.Text("Menu.Redo"), Redo, Shortcut(Key.Y, KeyModifiers.Control));
        var editItems = new List<object>
        {
            _undoMenuItem,
            _redoMenuItem,
            new Separator()
        };
        AddCommands(editItems, DemoCommandId.Translate, DemoCommandId.Rotate, DemoCommandId.Scale, DemoCommandId.Mirror, DemoCommandId.Copy);
        editItems.Add(new Separator());
        AddCommands(editItems, DemoCommandId.Delete);
        editItems.Add(MenuItem(DemoLocalization.Text("Menu.ClearSelection"), () =>
        {
            Session.Engine.ClearSelection();
            _viewport.RaiseSelectionChanged();
        }));
        editItems.Add(MenuItem(DemoLocalization.Text("Menu.ShowAll"), () => Session.Engine.ShowAll()));
        editItems.Add(MenuItem(DemoLocalization.Text("Menu.HideAll"), () => Session.Engine.HideAll()));
        var edit = Menu(MenuHeader("Menu.Edit"), editItems.ToArray());

        var drawItems = new List<object>();
        AddCommands(drawItems, DemoCommandId.Point, DemoCommandId.Line, DemoCommandId.Polyline);
        drawItems.Add(new Separator());
        AddCommands(drawItems, DemoCommandId.Circle, DemoCommandId.ArcThreePoints, DemoCommandId.ArcCenter, DemoCommandId.Ellipse);
        drawItems.Add(new Separator());
        AddCommands(drawItems, DemoCommandId.Rectangle, DemoCommandId.Polygon, DemoCommandId.Bezier, DemoCommandId.BSpline);
        var draw = Menu(MenuHeader("Menu.Draw"), drawItems.ToArray());

        var primitives = new List<object>();
        AddCommands(primitives, DemoCommandId.Box, DemoCommandId.Cylinder, DemoCommandId.Frustum, DemoCommandId.Cone,
            DemoCommandId.Torus, DemoCommandId.Sphere, DemoCommandId.Wedge, DemoCommandId.Pipe);
        var features = new List<object>();
        AddCommands(features, DemoCommandId.Extrude, DemoCommandId.Revolve, DemoCommandId.Sweep, DemoCommandId.Loft);
        var booleans = new List<object>();
        AddCommands(booleans, DemoCommandId.Fuse, DemoCommandId.Cut, DemoCommandId.Common, DemoCommandId.Section);
        var details = new List<object>();
        AddCommands(details, DemoCommandId.Fillet, DemoCommandId.Chamfer, DemoCommandId.Offset, DemoCommandId.Shell, DemoCommandId.Drill);
        var solid = Menu(MenuHeader("Menu.Solid"),
            Menu(MenuHeader("Menu.Primitives"), primitives.ToArray()),
            Menu(MenuHeader("Menu.Features"), features.ToArray()),
            Menu(MenuHeader("Menu.Boolean"), booleans.ToArray()),
            Menu(MenuHeader("Menu.Details"), details.ToArray()));

        var annotateItems = new List<object>();
        AddCommands(annotateItems, DemoCommandId.Text);
        annotateItems.Add(new Separator());
        AddCommands(annotateItems, DemoCommandId.LengthDimension, DemoCommandId.AngleDimension,
            DemoCommandId.RadiusDimension, DemoCommandId.DiameterDimension);
        var annotate = Menu(MenuHeader("Menu.Annotate"), annotateItems.ToArray());

        var toolItems = new List<object>();
        AddCommands(toolItems, DemoCommandId.AnalyzeBounds, DemoCommandId.AnalyzeMass, DemoCommandId.AnalyzeTopology,
            DemoCommandId.AnalyzeDistance, DemoCommandId.ValidateShape);
        var tools = Menu(MenuHeader("Menu.Tools"), toolItems.ToArray());

        var sampleItems = new List<object>();
        AddCommands(sampleItems, DemoCommandId.DemoElements, DemoCommandId.DemoGear, DemoCommandId.DemoManifold,
            DemoCommandId.DemoTwistedDuct);
        sampleItems.Add(new Separator());
        AddCommands(sampleItems, DemoCommandId.DemoBracket, DemoCommandId.DemoFlange, DemoCommandId.DemoAnnotations);
        var samples = Menu(MenuHeader("Menu.Samples"), sampleItems.ToArray());

        var language = Menu(MenuHeader("Menu.Language"),
            CheckMenuItem(DemoLocalization.Text("Menu.English"), DemoLocalization.CurrentLanguage == DemoLanguage.English,
                _ => SetLanguage(DemoLanguage.English), radio: true, groupName: "language"),
            CheckMenuItem(DemoLocalization.Text("Menu.Chinese"), DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified,
                _ => SetLanguage(DemoLanguage.ChineseSimplified), radio: true, groupName: "language"));

        var help = Menu(MenuHeader("Menu.Help"),
            AsyncMenuItem(DemoLocalization.Text("Menu.MouseHelp"), ShowMouseHelpAsync),
            AsyncMenuItem(DemoLocalization.Text("Menu.About"), ShowAboutAsync));

        _mainMenu.ItemsSource = new object[]
        {
            file, edit, draw, solid, annotate, BuildViewMenu(), tools, samples, language, help
        };
        UpdateHistoryUi();
    }

    private MenuItem BuildViewMenu()
    {
        var display = Menu(MenuHeader("Menu.Display"),
            MenuItem(DemoLocalization.Text("Menu.Shaded"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)),
            CheckMenuItem(DemoLocalization.Text("Menu.ShadedEdges"), true,
                item => ExecuteSafe(() => Session.Engine.SetFaceBoundariesVisible(item.IsChecked))),
            MenuItem(DemoLocalization.Text("Menu.Wireframe"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)),
            CheckMenuItem(DemoLocalization.Text("Menu.Hlr"), false,
                item => ExecuteSafe(() => Session.Engine.SetComputedHlr(item.IsChecked))),
            CheckMenuItem(DemoLocalization.Text("Menu.Antialiasing"), true,
                item => ExecuteSafe(() => Session.Engine.SetAntialiasing(item.IsChecked))),
            CheckMenuItem(DemoLocalization.Text("Menu.Triedron"), true,
                item => ExecuteSafe(() => Session.Engine.SetTriedronVisible(item.IsChecked))),
            CheckMenuItem(DemoLocalization.Text("Menu.ViewCube"), true,
                item => ExecuteSafe(() => Session.Engine.SetViewCubeVisible(item.IsChecked))));

        var standard = Menu(MenuHeader("Menu.StandardViews"),
            MenuItem(DemoLocalization.Text("Menu.Front"), () => Session.Engine.SetView(OcctViewOrientation.Front), Shortcut(Key.D1)),
            MenuItem(DemoLocalization.Text("Menu.Back"), () => Session.Engine.SetView(OcctViewOrientation.Back)),
            MenuItem(DemoLocalization.Text("Menu.Left"), () => Session.Engine.SetView(OcctViewOrientation.Left), Shortcut(Key.D2)),
            MenuItem(DemoLocalization.Text("Menu.Right"), () => Session.Engine.SetView(OcctViewOrientation.Right)),
            MenuItem(DemoLocalization.Text("Menu.Top"), () => Session.Engine.SetView(OcctViewOrientation.Top), Shortcut(Key.D3)),
            MenuItem(DemoLocalization.Text("Menu.Bottom"), () => Session.Engine.SetView(OcctViewOrientation.Bottom)),
            new Separator(),
            MenuItem(DemoLocalization.Text("Menu.Isometric"), () => Session.Engine.SetView(OcctViewOrientation.Isometric), Shortcut(Key.D0)),
            MenuItem(DemoLocalization.Text("Menu.NorthEast"), () => Session.SetIsoView(DemoIsoView.NorthEast)),
            MenuItem(DemoLocalization.Text("Menu.NorthWest"), () => Session.SetIsoView(DemoIsoView.NorthWest)),
            MenuItem(DemoLocalization.Text("Menu.SouthEast"), () => Session.SetIsoView(DemoIsoView.SouthEast)),
            MenuItem(DemoLocalization.Text("Menu.SouthWest"), () => Session.SetIsoView(DemoIsoView.SouthWest)));

        var projection = Menu(MenuHeader("Menu.Projection"),
            MenuItem(DemoLocalization.Text("Menu.Orthographic"), () => Session.Engine.SetProjection(OcctProjectionType.Orthographic)),
            MenuItem(DemoLocalization.Text("Menu.Perspective"), () => Session.Engine.SetProjection(OcctProjectionType.Perspective)),
            AsyncMenuItem(DemoLocalization.Text("Menu.PerspectiveFov"), SetPerspectiveFovAsync));

        return Menu(MenuHeader("Menu.View"),
            MenuItem(DemoLocalization.Text("Menu.FitAll"), () => Session.Engine.FitAll(), Shortcut(Key.F)),
            MenuItem(DemoLocalization.Text("Menu.FitSelected"), () =>
            {
                var shape = ActiveShape();
                if (shape is not null) Session.Engine.Fit(shape.Value);
            }),
            new Separator(),
            display,
            BuildDepthMenu(),
            standard,
            projection,
            AsyncMenuItem(DemoLocalization.Text("Menu.DisplayPrecision"), SetDisplayPrecisionAsync),
            BuildLightingMenu(),
            BuildMaterialMenu(),
            BuildSelectionMenu(),
            BuildSelectionAppearanceMenu(),
            CheckMenuItem(DemoLocalization.Text("Menu.WindowSelection"), _viewport.EnableRectangleSelection, item =>
            {
                _viewport.EnableRectangleSelection = item.IsChecked;
                _commandStatus.Text = DemoLocalization.Text(item.IsChecked ? "Status.WindowSelectionOn" : "Status.WindowSelectionOff");
            }),
            AsyncMenuItem(DemoLocalization.Text("Menu.SelectionTolerance"), SetSelectionToleranceAsync),
            AsyncMenuItem(DemoLocalization.Text("Menu.Background"), SetBackgroundColorAsync),
            MenuItem(DemoLocalization.Text("Menu.GradientBackground"), () =>
                Session.Engine.SetGradientBackground(DrawingColor.White, DrawingColor.LightSteelBlue)));
    }

    private MenuItem BuildDepthMenu()
    {
        return Menu(MenuHeader("Menu.DepthHandling"),
            CheckMenuItem(DemoLocalization.Text("Menu.AutoZFit"), _autoZFitEnabled, item => ExecuteSafe(() =>
            {
                _autoZFitEnabled = item.IsChecked;
                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                var message = DemoLocalization.Text(_autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                _commandStatus.Text = message;
                Log(message);
            })),
            MenuItem(DemoLocalization.Text("Menu.AutoZFitNow"), () => ExecuteSafe(Session.Engine.AutoZFit)),
            new Separator(),
            MenuItem(DemoLocalization.Text("Menu.DepthForward"), () => ApplyDepthBias(DemoDepthBiasPreset.Forward)),
            MenuItem(DemoLocalization.Text("Menu.DepthBackward"), () => ApplyDepthBias(DemoDepthBiasPreset.Backward)),
            MenuItem(DemoLocalization.Text("Menu.DepthReset"), () => ApplyDepthBias(DemoDepthBiasPreset.Default)));
    }

    private MenuItem BuildLightingMenu()
    {
        var items = new List<object>();
        foreach (var preset in Enum.GetValues<OcctLightingPreset>())
        {
            var captured = preset;
            items.Add(MenuItem(LightingPresetName(captured), () => ApplyLightingPreset(captured)));
        }
        items.Add(new Separator());
        items.Add(AsyncMenuItem(Local("Custom Lighting...", "自定义灯光..."), SetAdvancedLightingAsync));
        items.Add(MenuItem(Local("OCCT Default Lights", "恢复 OCCT 默认灯光"), () => ExecuteSafe(Session.Engine.ResetSceneLighting)));
        return Menu(Local("Lighting", "灯光"), items.ToArray());
    }

    private MenuItem BuildMaterialMenu()
    {
        var items = new List<object>();
        foreach (var material in Enum.GetValues<OcctMaterial>())
        {
            var captured = material;
            items.Add(AsyncMenuItem(MaterialDisplayName(captured), async () =>
            {
                var answer = await DialogService.ShowQuestionAsync(
                    this,
                    DemoLocalization.Text("Menu.Material"),
                    DemoLocalization.Text("Dialog.ApplyExistingMaterial"),
                    includeCancel: false);
                var apply = answer == DemoDialogChoice.Yes;
                ExecuteSafe(() => Session.Engine.SetDefaultMaterial(captured, apply));
                Log($"{DemoLocalization.Text("Menu.Material")}: {MaterialDisplayName(captured)}");
            }));
        }
        return Menu(MenuHeader("Menu.Material"), items.ToArray());
    }

    private MenuItem BuildSelectionMenu()
    {
        var items = new List<object>();
        foreach (var mode in Enum.GetValues<OcctSelectionMode>())
        {
            var captured = mode;
            items.Add(MenuItem(SelectionModeName(captured), () => SetSelectionMode(captured)));
        }
        return Menu(MenuHeader("Menu.SelectionMode"), items.ToArray());
    }

    private MenuItem BuildSelectionAppearanceMenu()
    {
        return Menu(Local("Selection Appearance", "选择外观"),
            AsyncMenuItem(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColorAsync),
            AsyncMenuItem(Local("Hover Color...", "悬浮高亮颜色..."), SetHoverHighlightColorAsync));
    }

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
        _toolbar.Children.Add(CommandButton(DemoLocalization.Text("Toolbar.Line"), DemoCommandId.Line));
        _toolbar.Children.Add(CommandButton(DemoLocalization.Text("Toolbar.Circle"), DemoCommandId.Circle));
        _toolbar.Children.Add(CommandButton(DemoLocalization.Text("Toolbar.Box"), DemoCommandId.Box));
        _toolbar.Children.Add(CommandButton(DemoLocalization.Text("Toolbar.Cylinder"), DemoCommandId.Cylinder));
        _toolbar.Children.Add(ToolSeparator());
        _toolbar.Children.Add(ToolButton(DemoLocalization.Text("Toolbar.Shaded"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        _toolbar.Children.Add(ToolButton(DemoLocalization.Text("Toolbar.Wireframe"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
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
            Width = 190,
            MinHeight = 30,
            Margin = new Thickness(2),
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

    private void AddCommands(ICollection<object> parent, params DemoCommandId[] commands)
    {
        foreach (var id in commands)
        {
            var definition = DemoLocalization.Localize(DemoCommandCatalog.Get(id));
            var item = AsyncMenuItem(definition.Text, () => RunCommandAsync(id), ShortcutFromText(definition.Shortcut));
            AvaloniaToolTip.SetTip(item, definition.Description);
            parent.Add(item);
        }
    }
}
