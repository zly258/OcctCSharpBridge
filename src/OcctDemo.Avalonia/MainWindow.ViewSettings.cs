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

        var root = new StackPanel { Margin = new Thickness(12), Spacing = 4 };

        void AddSection(string title, params Control[] rows)
        {
            root.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = global::Avalonia.Media.FontWeight.SemiBold,
                Margin = new Thickness(0, 8, 0, 2)
            });
            foreach (var row in rows)
                root.Children.Add(row);
        }

        AddSection(Local("Display", "显示"),
            Row(EnumCombo(Local("Display Style", "显示样式"), _visualStyle, ApplyVisualStyle,
                DemoVisualStyle.Wireframe, DemoVisualStyle.Shaded, DemoVisualStyle.ShadedEdges, DemoVisualStyle.HiddenLine)),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.Antialiasing"), true, v => Session.Engine.SetAntialiasing(v))),
            Row(AsyncViewSettingsButton(DemoLocalization.Text("Menu.DisplayPrecision"), SetDisplayPrecisionAsync),
                AsyncViewSettingsButton(DemoLocalization.Text("Menu.Background"), SetBackgroundColorAsync)),
            Row(AsyncViewSettingsButton(Local("Gradient First Color...", "渐变颜色一..."), () => PickGradientColorAsync(true)),
                AsyncViewSettingsButton(Local("Gradient Second Color...", "渐变颜色二..."), () => PickGradientColorAsync(false))),
            Row(EnumCombo(Local("Gradient Method", "渐变方式"), _gradientFillMethod,
                    v => { _gradientFillMethod = v; ApplyGradientBackground(); }),
                ViewSettingsButton(DemoLocalization.Text("Menu.GradientBackground"), ApplyGradientBackground)));

        AddSection(Local("Camera", "相机"),
            Row(ViewSettingsButton(DemoLocalization.Text("Menu.FitAll"), () => Session.Engine.FitAll()),
                ViewSettingsButton(DemoLocalization.Text("Menu.FitSelected"), () =>
                {
                    var s = ActiveShape();
                    if (s is not null) Session.Engine.Fit(s.Value);
                })),
            Row(ViewSettingsButton(Local("Zoom In", "放大"), () => Session.Engine.Zoom(1.2)),
                ViewSettingsButton(Local("Zoom Out", "缩小"), () => Session.Engine.Zoom(1.0 / 1.2))),
            Row(EnumCombo(Local("Projection", "投影"), _projectionType, SetProjectionMode),
                AsyncViewSettingsButton(DemoLocalization.Text("Menu.PerspectiveFov"), SetPerspectiveFovAsync)),
            Row(AsyncViewSettingsButton(Local("Zoom Sensitivity...", "缩放灵敏度..."), SetZoomSensitivityAsync)));

        AddSection(Local("Selection", "选择"),
            Row(EnumCombo(DemoLocalization.Text("Menu.SelectionMode"), OcctSelectionMode.Object, SetSelectionMode)),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.WindowSelection"),
                    (_viewport.InteractionFeatures & OcctViewportInteractionFeatures.RectangleSelection) != 0,
                    SetWindowSelectionEnabled),
                AsyncViewSettingsButton(DemoLocalization.Text("Menu.SelectionTolerance"), SetSelectionToleranceAsync)),
            Row(AsyncViewSettingsButton(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColorAsync),
                AsyncViewSettingsButton(Local("Hover Color...", "悬浮高亮颜色..."), SetHoverHighlightColorAsync)),
            Row(EnumCombo(Local("Selected Highlight Mode", "选中高亮模式"), _selectionHighlightMode, SetSelectionHighlightMode),
                EnumCombo(Local("Hover Highlight Mode", "悬浮高亮模式"), _hoverHighlightMode, SetHoverHighlightMode)));

        AddSection(Local("Helpers", "辅助"),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.Triedron"), true, v => Session.Engine.SetTriedronVisible(v)),
                ViewSettingsCheckBox(DemoLocalization.Text("Menu.ViewCube"), true, v => SetViewCubeVisible(v))),
            Row(ViewSettingsCheckBox(Local("ViewCube Axes", "ViewCube 坐标轴"), _viewCubeAxesVisible, SetViewCubeAxesVisible)),
            Row(EnumCombo(Local("Triedron Position", "坐标轴位置"), _triedronPosition, SetTriedronPosition),
                EnumCombo(Local("ViewCube Position", "ViewCube 位置"), _viewCubePosition, SetViewCubePosition)),
            Row(IntInput(Local("ViewCube Size (px)", "ViewCube 大小(px)"), _viewCubeSize, 10, 300, SetViewCubeSize),
                IntInput(Local("ViewCube Font Height", "ViewCube 文字大小"), _viewCubeFontHeight, 8, 48, SetViewCubeFontHeight)),
            Row(IntInput(Local("ViewCube Offset X (px)", "ViewCube 偏移 X(px)"), _viewCubeOffsetX, 0, 200, SetViewCubeOffsetX),
                IntInput(Local("ViewCube Offset Y (px)", "ViewCube 偏移 Y(px)"), _viewCubeOffsetY, 0, 200, SetViewCubeOffsetY)),
            Row(AsyncViewSettingsButton(Local("ViewCube Face Color...", "ViewCube 面颜色..."), PickViewCubeBoxColorAsync),
                AsyncViewSettingsButton(Local("ViewCube Highlight Color...", "ViewCube 高亮颜色..."), PickViewCubeFacetColorAsync)),
            Row(AsyncViewSettingsButton(Local("ViewCube Text Color...", "ViewCube 文字颜色..."), PickViewCubeTextColorAsync)));

        AddSection(Local("Appearance", "外观"),
            Row(EnumCombo(Local("Lighting Preset", "灯光预设"), OcctLightingPreset.Neutral,
                    p => ExecuteSafe(() => ApplyLightingPreset(p)))),
            Row(AsyncViewSettingsButton(Local("Custom Lighting...", "自定义灯光..."), SetAdvancedLightingAsync),
                ViewSettingsButton(Local("Reset Lighting", "重置灯光"), () => ExecuteSafe(Session.Engine.ResetSceneLighting))),
            Row(EnumCombo(Local("Material", "材质"), OcctMaterial.Default, material =>
                {
                    // apply to default only in Avalonia for simplicity
                    ExecuteSafe(() => Session.Engine.SetDefaultMaterial(material, false));
                    Log($"{DemoLocalization.Text("Menu.Material")}: {MaterialDisplayName(material)}");
                })),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.AutoZFit"), _autoZFitEnabled, v =>
                    ExecuteSafe(() =>
                    {
                        _autoZFitEnabled = v;
                        Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                    })),
                ViewSettingsButton(DemoLocalization.Text("Menu.AutoZFitNow"), () => ExecuteSafe(Session.Engine.AutoZFit))),
            Row(EnumCombo(Local("Depth Bias", "深度偏移"), DemoDepthBiasPreset.Default,
                    p => ExecuteSafe(() => ApplyDepthBias(p)))));

        var window = new Window
        {
            Title = Local("View Settings", "视图设置"),
            Width = 560,
            Height = 640,
            MinWidth = 480,
            MinHeight = 400,
            Content = new ScrollViewer
            {
                Content = root,
                VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            },
            WindowStartupLocation = WindowStartupLocation.CenterOwner
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
        var b = new Button { Content = text, HorizontalAlignment = HorizontalAlignment.Stretch };
        b.Click += (_, _) => action();
        return b;
    }

    private static Button AsyncViewSettingsButton(string text, Func<Task> action)
    {
        var b = new Button { Content = text, HorizontalAlignment = HorizontalAlignment.Stretch };
        b.Click += async (_, _) => await action();
        return b;
    }

    private static CheckBox ViewSettingsCheckBox(string text, bool initial, Action<bool> action)
    {
        var cb = new CheckBox { Content = text, IsChecked = initial };
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
