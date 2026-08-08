using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CadCommon;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using AvaloniaBrushes = Avalonia.Media.Brushes;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaFontFamily = Avalonia.Media.FontFamily;
using AvaloniaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using AvaloniaOrientation = Avalonia.Layout.Orientation;
using AvaloniaToolTip = Avalonia.Controls.ToolTip;
using Forms = System.Windows.Forms;
using Button = Avalonia.Controls.Button;
using CheckBox = Avalonia.Controls.CheckBox;
using ContextMenu = Avalonia.Controls.ContextMenu;
using MenuItem = Avalonia.Controls.MenuItem;
using ComboBox = Avalonia.Controls.ComboBox;
using Control = Avalonia.Controls.Control;
using GroupBox = Avalonia.Controls.GroupBox;
using TextBox = Avalonia.Controls.TextBox;
using TreeView = Avalonia.Controls.TreeView;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace CadAvalonia;

public sealed partial class MainWindow
{
    private void BuildMenus()
    {
        var file = Menu(MenuHeader("Menu.File"),
            MenuItem(CadLocalization.Text("Menu.New"), NewDocument, Shortcut(Key.N, KeyModifiers.Control)),
            MenuItem(CadLocalization.Text("Menu.Open"), OpenDocument, Shortcut(Key.O, KeyModifiers.Control)),
            MenuItem(CadLocalization.Text("Menu.Save"), () => SaveDocument(false), Shortcut(Key.S, KeyModifiers.Control)),
            MenuItem(CadLocalization.Text("Menu.SaveAs"), () => SaveDocument(true), Shortcut(Key.S, KeyModifiers.Control | KeyModifiers.Shift)),
            new Separator(),
            MenuItem(CadLocalization.Text("Menu.Import"), ImportDocument),
            MenuItem(CadLocalization.Text("Menu.ExportSelected"), ExportSelected),
            MenuItem(CadLocalization.Text("Menu.ExportImage"), ExportViewImage),
            new Separator(),
            MenuItem(CadLocalization.Text("Menu.Exit"), Close));

        _undoMenuItem = MenuItem(CadLocalization.Text("Menu.Undo"), Undo, Shortcut(Key.Z, KeyModifiers.Control));
        _redoMenuItem = MenuItem(CadLocalization.Text("Menu.Redo"), Redo, Shortcut(Key.Y, KeyModifiers.Control));
        var editItems = new List<object>
        {
            _undoMenuItem,
            _redoMenuItem,
            new Separator()
        };
        AddCommands(editItems, CadCommandId.Translate, CadCommandId.Rotate, CadCommandId.Scale, CadCommandId.Mirror, CadCommandId.Copy);
        editItems.Add(new Separator());
        AddCommands(editItems, CadCommandId.Delete);
        editItems.Add(MenuItem(CadLocalization.Text("Menu.ClearSelection"), () =>
        {
            Session.Engine.ClearSelection();
            _viewport.RaiseSelectionChanged();
        }));
        editItems.Add(MenuItem(CadLocalization.Text("Menu.ShowAll"), () => Session.Engine.ShowAll()));
        editItems.Add(MenuItem(CadLocalization.Text("Menu.HideAll"), () => Session.Engine.HideAll()));
        var edit = Menu(MenuHeader("Menu.Edit"), editItems.ToArray());

        var drawItems = new List<object>();
        AddCommands(drawItems, CadCommandId.Point, CadCommandId.Line, CadCommandId.Polyline);
        drawItems.Add(new Separator());
        AddCommands(drawItems, CadCommandId.Circle, CadCommandId.ArcThreePoints, CadCommandId.ArcCenter, CadCommandId.Ellipse);
        drawItems.Add(new Separator());
        AddCommands(drawItems, CadCommandId.Rectangle, CadCommandId.Polygon, CadCommandId.Bezier, CadCommandId.BSpline);
        var draw = Menu(MenuHeader("Menu.Draw"), drawItems.ToArray());

        var primitives = new List<object>();
        AddCommands(primitives, CadCommandId.Box, CadCommandId.Cylinder, CadCommandId.Frustum, CadCommandId.Cone,
            CadCommandId.Torus, CadCommandId.Sphere, CadCommandId.Wedge, CadCommandId.Pipe);
        var features = new List<object>();
        AddCommands(features, CadCommandId.Extrude, CadCommandId.Revolve, CadCommandId.Sweep, CadCommandId.Loft);
        var booleans = new List<object>();
        AddCommands(booleans, CadCommandId.Fuse, CadCommandId.Cut, CadCommandId.Common, CadCommandId.Section);
        var details = new List<object>();
        AddCommands(details, CadCommandId.Fillet, CadCommandId.Chamfer, CadCommandId.Offset, CadCommandId.Shell, CadCommandId.Drill);
        var solid = Menu(MenuHeader("Menu.Solid"),
            Menu(MenuHeader("Menu.Primitives"), primitives.ToArray()),
            Menu(MenuHeader("Menu.Features"), features.ToArray()),
            Menu(MenuHeader("Menu.Boolean"), booleans.ToArray()),
            Menu(MenuHeader("Menu.Details"), details.ToArray()));

        var annotateItems = new List<object>();
        AddCommands(annotateItems, CadCommandId.Text);
        annotateItems.Add(new Separator());
        AddCommands(annotateItems, CadCommandId.LengthDimension, CadCommandId.AngleDimension,
            CadCommandId.RadiusDimension, CadCommandId.DiameterDimension);
        var annotate = Menu(MenuHeader("Menu.Annotate"), annotateItems.ToArray());

        var toolItems = new List<object>();
        AddCommands(toolItems, CadCommandId.AnalyzeBounds, CadCommandId.AnalyzeMass, CadCommandId.AnalyzeTopology,
            CadCommandId.AnalyzeDistance, CadCommandId.ValidateShape);
        var tools = Menu(MenuHeader("Menu.Tools"), toolItems.ToArray());

        var sampleItems = new List<object>();
        AddCommands(sampleItems, CadCommandId.DemoElements, CadCommandId.DemoGear, CadCommandId.DemoManifold,
            CadCommandId.DemoTwistedDuct);
        sampleItems.Add(new Separator());
        AddCommands(sampleItems, CadCommandId.DemoBracket, CadCommandId.DemoFlange, CadCommandId.DemoAnnotations);
        var samples = Menu(MenuHeader("Menu.Samples"), sampleItems.ToArray());

        var language = Menu(MenuHeader("Menu.Language"),
            CheckMenuItem(CadLocalization.Text("Menu.English"), CadLocalization.CurrentLanguage == CadLanguage.English,
                _ => SetLanguage(CadLanguage.English), radio: true, groupName: "language"),
            CheckMenuItem(CadLocalization.Text("Menu.Chinese"), CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified,
                _ => SetLanguage(CadLanguage.ChineseSimplified), radio: true, groupName: "language"));

        var help = Menu(MenuHeader("Menu.Help"),
            MenuItem(CadLocalization.Text("Menu.MouseHelp"), ShowMouseHelp),
            MenuItem(CadLocalization.Text("Menu.About"), ShowAbout));

        _mainMenu.ItemsSource = new object[]
        {
            file, edit, draw, solid, annotate, BuildViewMenu(), tools, samples, language, help
        };
        UpdateHistoryUi();
    }

    private MenuItem BuildViewMenu()
    {
        var display = Menu(MenuHeader("Menu.Display"),
            MenuItem(CadLocalization.Text("Menu.Shaded"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)),
            CheckMenuItem(CadLocalization.Text("Menu.ShadedEdges"), true,
                item => ExecuteSafe(() => Session.Engine.SetFaceBoundariesVisible(item.IsChecked))),
            MenuItem(CadLocalization.Text("Menu.Wireframe"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)),
            CheckMenuItem(CadLocalization.Text("Menu.Hlr"), false,
                item => ExecuteSafe(() => Session.Engine.SetComputedHlr(item.IsChecked))),
            CheckMenuItem(CadLocalization.Text("Menu.Antialiasing"), true,
                item => ExecuteSafe(() => Session.Engine.SetAntialiasing(item.IsChecked))),
            CheckMenuItem(CadLocalization.Text("Menu.Triedron"), true,
                item => ExecuteSafe(() => Session.Engine.SetTriedronVisible(item.IsChecked))),
            CheckMenuItem(CadLocalization.Text("Menu.ViewCube"), true,
                item => ExecuteSafe(() => Session.Engine.SetViewCubeVisible(item.IsChecked))));

        var standard = Menu(MenuHeader("Menu.StandardViews"),
            MenuItem(CadLocalization.Text("Menu.Front"), () => Session.Engine.SetView(OcctViewOrientation.Front), Shortcut(Key.D1)),
            MenuItem(CadLocalization.Text("Menu.Back"), () => Session.Engine.SetView(OcctViewOrientation.Back)),
            MenuItem(CadLocalization.Text("Menu.Left"), () => Session.Engine.SetView(OcctViewOrientation.Left), Shortcut(Key.D2)),
            MenuItem(CadLocalization.Text("Menu.Right"), () => Session.Engine.SetView(OcctViewOrientation.Right)),
            MenuItem(CadLocalization.Text("Menu.Top"), () => Session.Engine.SetView(OcctViewOrientation.Top), Shortcut(Key.D3)),
            MenuItem(CadLocalization.Text("Menu.Bottom"), () => Session.Engine.SetView(OcctViewOrientation.Bottom)),
            new Separator(),
            MenuItem(CadLocalization.Text("Menu.Isometric"), () => Session.Engine.SetView(OcctViewOrientation.Isometric), Shortcut(Key.D0)),
            MenuItem(CadLocalization.Text("Menu.NorthEast"), () => Session.SetIsoView(CadIsoView.NorthEast)),
            MenuItem(CadLocalization.Text("Menu.NorthWest"), () => Session.SetIsoView(CadIsoView.NorthWest)),
            MenuItem(CadLocalization.Text("Menu.SouthEast"), () => Session.SetIsoView(CadIsoView.SouthEast)),
            MenuItem(CadLocalization.Text("Menu.SouthWest"), () => Session.SetIsoView(CadIsoView.SouthWest)));

        var projection = Menu(MenuHeader("Menu.Projection"),
            MenuItem(CadLocalization.Text("Menu.Orthographic"), () => Session.Engine.SetProjection(OcctProjectionType.Orthographic)),
            MenuItem(CadLocalization.Text("Menu.Perspective"), () => Session.Engine.SetProjection(OcctProjectionType.Perspective)),
            AsyncMenuItem(CadLocalization.Text("Menu.PerspectiveFov"), SetPerspectiveFovAsync));

        return Menu(MenuHeader("Menu.View"),
            MenuItem(CadLocalization.Text("Menu.FitAll"), () => Session.Engine.FitAll(), Shortcut(Key.F)),
            MenuItem(CadLocalization.Text("Menu.FitSelected"), () =>
            {
                var shape = ActiveShape();
                if (shape is not null) Session.Engine.Fit(shape.Value);
            }),
            new Separator(),
            display,
            BuildDepthMenu(),
            standard,
            projection,
            AsyncMenuItem(CadLocalization.Text("Menu.DisplayPrecision"), SetDisplayPrecisionAsync),
            BuildLightingMenu(),
            BuildMaterialMenu(),
            BuildSelectionMenu(),
            BuildSelectionAppearanceMenu(),
            CheckMenuItem(CadLocalization.Text("Menu.WindowSelection"), _viewport.EnableRectangleSelection, item =>
            {
                _viewport.EnableRectangleSelection = item.IsChecked;
                _commandStatus.Text = CadLocalization.Text(item.IsChecked ? "Status.WindowSelectionOn" : "Status.WindowSelectionOff");
            }),
            AsyncMenuItem(CadLocalization.Text("Menu.SelectionTolerance"), SetSelectionToleranceAsync),
            MenuItem(CadLocalization.Text("Menu.Background"), SetBackgroundColor),
            MenuItem(CadLocalization.Text("Menu.GradientBackground"), () =>
                Session.Engine.SetGradientBackground(DrawingColor.White, DrawingColor.LightSteelBlue)));
    }

    private MenuItem BuildDepthMenu()
    {
        return Menu(MenuHeader("Menu.DepthHandling"),
            CheckMenuItem(CadLocalization.Text("Menu.AutoZFit"), _autoZFitEnabled, item => ExecuteSafe(() =>
            {
                _autoZFitEnabled = item.IsChecked;
                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                var message = CadLocalization.Text(_autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                _commandStatus.Text = message;
                Log(message);
            })),
            MenuItem(CadLocalization.Text("Menu.AutoZFitNow"), () => ExecuteSafe(Session.Engine.AutoZFit)),
            new Separator(),
            MenuItem(CadLocalization.Text("Menu.DepthForward"), () => ApplyDepthBias(CadDepthBiasPreset.Forward)),
            MenuItem(CadLocalization.Text("Menu.DepthBackward"), () => ApplyDepthBias(CadDepthBiasPreset.Backward)),
            MenuItem(CadLocalization.Text("Menu.DepthReset"), () => ApplyDepthBias(CadDepthBiasPreset.Default)));
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
            items.Add(MenuItem(MaterialDisplayName(captured), () =>
            {
                var apply = Forms.MessageBox.Show(
                    CadLocalization.Text("Dialog.ApplyExistingMaterial"),
                    CadLocalization.Text("Menu.Material"),
                    Forms.MessageBoxButtons.YesNo,
                    Forms.MessageBoxIcon.Question) == Forms.DialogResult.Yes;
                ExecuteSafe(() => Session.Engine.SetDefaultMaterial(captured, apply));
                Log($"{CadLocalization.Text("Menu.Material")}: {MaterialDisplayName(captured)}");
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
            MenuItem(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColor),
            MenuItem(Local("Hover Color...", "悬浮高亮颜色..."), SetHoverHighlightColor));
    }

    private void BuildToolbar()
    {
        var selectedIndex = _selectionCombo?.SelectedIndex ?? 0;
        _toolbar.Children.Clear();
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.New"), NewDocument));
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Open"), OpenDocument));
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Save"), () => SaveDocument(false)));
        _toolbar.Children.Add(ToolSeparator());
        _undoButton = ToolButton(CadLocalization.Text("Toolbar.Undo"), Undo);
        _redoButton = ToolButton(CadLocalization.Text("Toolbar.Redo"), Redo);
        _toolbar.Children.Add(_undoButton);
        _toolbar.Children.Add(_redoButton);
        _toolbar.Children.Add(ToolSeparator());
        _toolbar.Children.Add(CommandButton(CadLocalization.Text("Toolbar.Line"), CadCommandId.Line));
        _toolbar.Children.Add(CommandButton(CadLocalization.Text("Toolbar.Circle"), CadCommandId.Circle));
        _toolbar.Children.Add(CommandButton(CadLocalization.Text("Toolbar.Box"), CadCommandId.Box));
        _toolbar.Children.Add(CommandButton(CadLocalization.Text("Toolbar.Cylinder"), CadCommandId.Cylinder));
        _toolbar.Children.Add(ToolSeparator());
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Shaded"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Wireframe"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Extents"), () => Session.Engine.FitAll()));
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Isometric"), () => Session.Engine.SetView(OcctViewOrientation.Isometric)));
        _toolbar.Children.Add(ToolSeparator());
        _toolbar.Children.Add(new TextBlock
        {
            Text = CadLocalization.Text("Toolbar.Selection"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 0, 2, 0)
        });
        _selectionCombo = new ComboBox
        {
            Width = 130,
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

    private void AddCommands(ICollection<object> parent, params CadCommandId[] commands)
    {
        foreach (var id in commands)
        {
            var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));
            var item = AsyncMenuItem(definition.Text, () => RunCommandAsync(id), ShortcutFromText(definition.Shortcut));
            AvaloniaToolTip.SetTip(item, definition.Description);
            parent.Add(item);
        }
    }
}
