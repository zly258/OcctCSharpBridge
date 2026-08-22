using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private Window? _advancedViewSettingsWindow;
    private DrawingColor _gradientFirstColor = DrawingColor.White;
    private DrawingColor _gradientSecondColor = DrawingColor.LightSteelBlue;
    private OcctGradientFillMethod _gradientFillMethod = OcctGradientFillMethod.Vertical;

    private void ShowAdvancedViewSettingsWindow()
    {
        if (_advancedViewSettingsWindow is { IsVisible: true })
        {
            _advancedViewSettingsWindow.Activate();
            return;
        }

        // ── Camera tab ────────────────────────────────────────────────────────────
        var cameraTab = ViewSettingsTab(Local("Camera", "相机"),
            Row(ViewSettingsButton(DemoLocalization.Text("Menu.FitAll"),      () => Session.Engine.FitAll()),
                ViewSettingsButton(DemoLocalization.Text("Menu.FitSelected"), () => { var s = ActiveShape(); if (s is not null) Session.Engine.Fit(s.Value); })),
            Row(ViewSettingsButton(Local("Zoom In",  "放大"), () => Session.Engine.Zoom(1.2)),
                ViewSettingsButton(Local("Zoom Out", "缩小"), () => Session.Engine.Zoom(1.0 / 1.2))),
            Row(EnumCombo(Local("Projection", "投影"), _projectionType, SetProjectionMode)),
            Row(AsyncViewSettingsButton(DemoLocalization.Text("Menu.PerspectiveFov"),     SetPerspectiveFovAsync),
                AsyncViewSettingsButton(Local("Zoom Sensitivity...", "缩放灵敏度..."),     SetZoomSensitivityAsync)));

        // ── Display tab ───────────────────────────────────────────────────────────
        var displayTab = ViewSettingsTab(Local("Display", "显示"),
            Row(EnumCombo(Local("Display Style", "显示样式"), _displayMode, SetDisplayStyle)),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.ShadedEdges"),  true,  v => Session.Engine.SetFaceBoundariesVisible(v)),
                ViewSettingsCheckBox(DemoLocalization.Text("Menu.Hlr"),          false, v => Session.Engine.SetComputedHlr(v))),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.Antialiasing"), true,  v => Session.Engine.SetAntialiasing(v))),
            Row(AsyncViewSettingsButton(DemoLocalization.Text("Menu.DisplayPrecision"), SetDisplayPrecisionAsync)),
            Row(AsyncViewSettingsButton(DemoLocalization.Text("Menu.Background"),       SetBackgroundColorAsync),
                AsyncViewSettingsButton(Local("Gradient First Color...", "渐变颜色一..."), () => PickGradientColorAsync(true))),
            Row(AsyncViewSettingsButton(Local("Gradient Second Color...", "渐变颜色二..."), () => PickGradientColorAsync(false)),
                ViewSettingsButton(DemoLocalization.Text("Menu.GradientBackground"), ApplyGradientBackground)),
            Row(EnumCombo(Local("Gradient Method", "渐变方式"), _gradientFillMethod,
                    v => { _gradientFillMethod = v; ApplyGradientBackground(); })));

        // ── Selection tab ─────────────────────────────────────────────────────────
        var selectionTab = ViewSettingsTab(Local("Selection", "选择"),
            Row(EnumCombo(DemoLocalization.Text("Menu.SelectionMode"), (OcctSelectionMode)0, SetSelectionMode)),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.WindowSelection"),
                    (_viewport.InteractionFeatures & OcctViewportInteractionFeatures.RectangleSelection) != 0,
                    SetWindowSelectionEnabled)),
            Row(AsyncViewSettingsButton(DemoLocalization.Text("Menu.SelectionTolerance"), SetSelectionToleranceAsync)),
            Row(AsyncViewSettingsButton(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColorAsync),
                AsyncViewSettingsButton(Local("Hover Color...", "悬浮高亮颜色..."),   SetHoverHighlightColorAsync)),
            Row(EnumCombo(Local("Selected Highlight Mode", "选中高亮模式"), _selectionHighlightMode, SetSelectionHighlightMode)),
            Row(EnumCombo(Local("Hover Highlight Mode",    "悬浮高亮模式"), _hoverHighlightMode,    SetHoverHighlightMode)));

        // ── Helpers tab ───────────────────────────────────────────────────────────
        var helpersTab = ViewSettingsTab(Local("Helpers", "辅助"),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.Triedron"), true, v => Session.Engine.SetTriedronVisible(v)),
                ViewSettingsCheckBox(DemoLocalization.Text("Menu.ViewCube"), true, v => SetViewCubeVisible(v))),
            Row(EnumCombo(Local("Triedron Position",  "坐标轴位置"),    _triedronPosition,  SetTriedronPosition)),
            Row(EnumCombo(Local("ViewCube Position",  "ViewCube 位置"), _viewCubePosition,  SetViewCubePosition)),
            Row(IntInput(Local("ViewCube Size (px)",  "ViewCube 大小(px)"),   _viewCubeSize,  10, 300, SetViewCubeSize),
                IntInput(Local("ViewCube Offset (px)","ViewCube 偏移(px)"),   _viewCubeOffset, 0, 200, v => SetViewCubeOffset(v, v))),
            Row(IntInput(Local("ViewCube Font Size (pt)", "ViewCube 字体大小(pt)"), (int)_viewCubeFontHeight, 6, 36, v => SetViewCubeFontHeight(v)),
                FontCombo(Local("ViewCube Font", "ViewCube 字体"), _viewCubeFontName, SetViewCubeFontName)),
            Row(AsyncViewSettingsButton(Local("ViewCube Text Color...", "文字颜色..."), () => PickViewCubeColorAsync(0)),
                AsyncViewSettingsButton(Local("ViewCube Box Color...", "背景颜色..."), () => PickViewCubeColorAsync(1))),
            Row(AsyncViewSettingsButton(Local("ViewCube Facet Color...", "面高亮颜色..."), () => PickViewCubeColorAsync(2)),
                ViewSettingsButton(Local("Reset ViewCube", "重置 ViewCube"), ResetViewCubeAppearance)));

        // ── Appearance tab ────────────────────────────────────────────────────────
        var appearanceTab = ViewSettingsTab(Local("Appearance", "外观"),
            Row(EnumCombo(Local("Lighting Preset", "灯光预设"), OcctLightingPreset.Studio,
                    p => ExecuteSafe(() => ApplyLightingPreset(p)))),
            Row(AsyncViewSettingsButton(Local("Custom Lighting...", "自定义灯光..."),  SetAdvancedLightingAsync),
                ViewSettingsButton(Local("Reset Lighting",          "重置灯光"),       () => ExecuteSafe(Session.Engine.ResetSceneLighting))),
            Row(EnumCombo(Local("Material", "材质"), OcctMaterial.Default, async material =>
                {
                    var answer = await DialogService.ShowQuestionAsync(
                        this,
                        DemoLocalization.Text("Menu.Material"),
                        DemoLocalization.Text("Dialog.ApplyExistingMaterial"),
                        includeCancel: false);
                    var apply = answer == DemoDialogChoice.Yes;
                    ExecuteSafe(() => Session.Engine.SetDefaultMaterial(material, apply));
                    Log($"{DemoLocalization.Text("Menu.Material")}: {MaterialDisplayName(material)}");
                })),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.AutoZFit"), _autoZFitEnabled, v =>
                    ExecuteSafe(() =>
                    {
                        _autoZFitEnabled = v;
                        Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                        var msg = DemoLocalization.Text(_autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                        _commandStatus.Text = msg;
                        Log(msg);
                    })),
                ViewSettingsButton(DemoLocalization.Text("Menu.AutoZFitNow"), () => ExecuteSafe(Session.Engine.AutoZFit))),
            Row(EnumCombo(Local("Depth Bias", "深度偏移/防闪烁"), OcctDepthBiasPreset.Default,
                    p => ExecuteSafe(() => ApplyDepthBias(p)))));

        var tabs = new TabControl
        {
            ItemsSource = new object[] { cameraTab, displayTab, selectionTab, helpersTab, appearanceTab }
        };

        var window = new Window
        {
            Title = Local("View Settings", "视图设置"),
            Width = 560,
            Height = 660,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = tabs
        };
        _advancedViewSettingsWindow = window;
        window.Closed += (_, _) => _advancedViewSettingsWindow = null;
        window.Show(this);
    }

    // ── Layout helpers ────────────────────────────────────────────────────────────

    private const double ColWidth = 250.0;

    private static Grid Row(params Control[] cells)
    {
        var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) };
        grid.ColumnDefinitions.Add(new ColumnDefinition(ColWidth, GridUnitType.Pixel));
        grid.ColumnDefinitions.Add(new ColumnDefinition(ColWidth, GridUnitType.Pixel));
        for (var i = 0; i < cells.Length && i < 2; i++)
        {
            Grid.SetColumn(cells[i], i);
            cells[i].Margin = i == 0
                ? new Thickness(0, 0, 4, 0)
                : new Thickness(4, 0, 0, 0);
            grid.Children.Add(cells[i]);
        }
        return grid;
    }

    private static TabItem ViewSettingsTab(string text, params Grid[] rows)
    {
        var panel = new StackPanel { Margin = new Thickness(12), Spacing = 2 };
        foreach (var row in rows) panel.Children.Add(row);
        return new TabItem
        {
            Header  = text,
            Content = new ScrollViewer { Content = panel }
        };
    }

    private static Button ViewSettingsButton(string text, Action action)
    {
        var b = new Button { Content = text, Height = 28, HorizontalAlignment = HorizontalAlignment.Stretch };
        b.Click += (_, _) => action();
        return b;
    }

    private static Button AsyncViewSettingsButton(string text, Func<Task> action)
    {
        var b = new Button { Content = text, Height = 28, HorizontalAlignment = HorizontalAlignment.Stretch };
        b.Click += async (_, _) => await action();
        return b;
    }

    private static CheckBox ViewSettingsCheckBox(string text, bool initial, Action<bool> action)
    {
        var cb = new CheckBox { Content = text, IsChecked = initial, Height = 28 };
        cb.IsCheckedChanged += (_, _) => action(cb.IsChecked == true);
        return cb;
    }

    private static StackPanel EnumCombo<TEnum>(string label, TEnum current, Action<TEnum> apply, params TEnum[] values)
        where TEnum : struct, Enum
    {
        var p     = new StackPanel { Orientation = Orientation.Vertical };
        var combo = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemsSource         = values.Length == 0 ? Enum.GetValues<TEnum>() : values
        };
        combo.SelectedItem       = current;
        combo.SelectionChanged  += (_, _) => { if (combo.SelectedItem is TEnum v) apply(v); };
        p.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 2) });
        p.Children.Add(combo);
        return p;
    }

    private static StackPanel EnumCombo<TEnum>(string label, TEnum current, Func<TEnum, Task> apply, params TEnum[] values)
        where TEnum : struct, Enum
    {
        // Dispatch to the synchronous overload. A bare lambda here would bind to
        // THIS overload again (Func<TEnum, Task> matches better than Action<TEnum>)
        // and recurse forever, so cast explicitly to Action.
        return EnumCombo(label, current, new Action<TEnum>(async v =>
        {
            try { await apply(v); }
            catch (Exception) { /* dialog/cancellation handled by the caller */ }
        }), values);
    }

    private static StackPanel IntInput(string label, int initial, int min, int max, Action<int> apply)
    {
        var p  = new StackPanel { Orientation = Orientation.Vertical };
        var nud = new NumericUpDown
        {
            Minimum             = min,
            Maximum             = max,
            Value               = Math.Clamp(initial, min, max),
            Increment           = 1,
            FormatString        = "0",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height              = 28
        };
        nud.ValueChanged += (_, _) =>
        {
            if (nud.Value.HasValue)
                apply((int)nud.Value.Value);
        };
        p.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 2) });
        p.Children.Add(nud);
        return p;
    }

    private static StackPanel FontCombo(string label, string current, Action<string> apply)
    {
        var p     = new StackPanel { Orientation = Orientation.Vertical };
        var combo = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemsSource         = new[] { "Segoe UI", "Microsoft YaHei", "Arial", "Calibri", "Tahoma", "Consolas", "SimSun", "SimHei" }
        };
        combo.SelectedItem      = current;
        combo.SelectionChanged += (_, _) => { if (combo.SelectedItem is string f) apply(f); };
        p.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 2) });
        p.Children.Add(combo);
        return p;
    }

    private async Task PickViewCubeColorAsync(int type)
    {
        var current = type switch
        {
            0 => _viewCubeTextColor,
            1 => _viewCubeBoxColor,
            _ => _viewCubeFacetColor
        };
        var title = type switch
        {
            0 => Local("ViewCube Text Color", "ViewCube 文字颜色"),
            1 => Local("ViewCube Box Color", "ViewCube 背景颜色"),
            _ => Local("ViewCube Facet Color", "ViewCube 面高亮颜色")
        };
        var color = await ClassicColorDialog.ShowAsync(this, title, current);
        if (color is null) return;
        switch (type)
        {
            case 0: SetViewCubeTextColor(color.Value); break;
            case 1: SetViewCubeBoxColor(color.Value); break;
            default: SetViewCubeFacetColor(color.Value); break;
        }
    }

    private async Task PickGradientColorAsync(bool first)
    {
        var initial = first ? _gradientFirstColor : _gradientSecondColor;
        var color = await ClassicColorDialog.ShowAsync(
            this,
            first ? Local("Gradient First Color", "渐变颜色一") : Local("Gradient Second Color", "渐变颜色二"),
            initial);
        if (color is null) return;
        if (first) _gradientFirstColor = color.Value; else _gradientSecondColor = color.Value;
        ApplyGradientBackground();
    }

    private void ApplyGradientBackground()
    {
        ExecuteSafe(() =>
        {
            Session.Engine.SetGradientBackground(_gradientFirstColor, _gradientSecondColor, _gradientFillMethod);
            Log($"{DemoLocalization.Text("Menu.GradientBackground")}: {_gradientFillMethod}");
            _viewport.RefreshNativeView();
        });
    }
}
