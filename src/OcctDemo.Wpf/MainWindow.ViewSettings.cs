using System.Globalization;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using Controls = System.Windows.Controls;
using System.Windows;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private Window? _advancedViewSettingsWindow;
    private DrawingColor _gradientFirstColor = DrawingColor.White;
    private DrawingColor _gradientSecondColor = DrawingColor.LightSteelBlue;
    private OcctGradientFillMethod _gradientFillMethod = OcctGradientFillMethod.Vertical;

    private void ShowAdvancedViewSettingsWindow()
    {
        if (_advancedViewSettingsWindow is not null)
        {
            _advancedViewSettingsWindow.Activate();
            return;
        }

        var root = new Controls.StackPanel { Margin = new System.Windows.Thickness(12) };

        void AddSection(string title, params Controls.Grid[] rows)
        {
            root.Children.Add(new Controls.TextBlock
            {
                Text = title,
                FontWeight = System.Windows.FontWeights.SemiBold,
                Margin = new System.Windows.Thickness(0, 10, 0, 4)
            });
            foreach (var row in rows)
                root.Children.Add(row);
        }

        AddSection(Local("Display", "显示"),
            Row(EnumCombo(Local("Display Style", "显示样式"), _visualStyle, ApplyVisualStyle,
                DemoVisualStyle.Wireframe, DemoVisualStyle.Shaded, DemoVisualStyle.ShadedEdges, DemoVisualStyle.HiddenLine)),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.Antialiasing"), true, v => Session.Engine.SetAntialiasing(v))),
            Row(ViewSettingsButton(DemoLocalization.Text("Menu.DisplayPrecision"), SetDisplayPrecision),
                ViewSettingsButton(DemoLocalization.Text("Menu.Background"), SetBackgroundColor)),
            Row(ViewSettingsButton(Local("Gradient First Color...", "渐变颜色一..."), () => PickGradientColor(true)),
                ViewSettingsButton(Local("Gradient Second Color...", "渐变颜色二..."), () => PickGradientColor(false))),
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
                ViewSettingsButton(DemoLocalization.Text("Menu.PerspectiveFov"), SetPerspectiveFov)),
            Row(ViewSettingsButton(Local("Zoom Sensitivity...", "缩放灵敏度..."), SetZoomSensitivity)));

        AddSection(Local("Selection", "选择"),
            Row(EnumCombo(DemoLocalization.Text("Menu.SelectionMode"), OcctSelectionMode.Object, SetSelectionMode)),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.WindowSelection"),
                    (Viewport.InteractionFeatures & OcctViewportInteractionFeatures.RectangleSelection) != 0,
                    SetWindowSelectionEnabled),
                ViewSettingsButton(DemoLocalization.Text("Menu.SelectionTolerance"), SetSelectionTolerance)),
            Row(ViewSettingsButton(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColor),
                ViewSettingsButton(Local("Hover Color...", "悬浮高亮颜色..."), SetHoverHighlightColor)),
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
            Row(ViewSettingsButton(Local("ViewCube Face Color...", "ViewCube 面颜色..."), PickViewCubeBoxColor),
                ViewSettingsButton(Local("ViewCube Highlight Color...", "ViewCube 高亮颜色..."), PickViewCubeFacetColor)),
            Row(ViewSettingsButton(Local("ViewCube Text Color...", "ViewCube 文字颜色..."), PickViewCubeTextColor)));

        AddSection(Local("Appearance", "外观"),
            Row(EnumCombo(Local("Lighting Preset", "灯光预设"), OcctLightingPreset.Neutral,
                    p => ExecuteSafe(() => ApplyLightingPreset(p)))),
            Row(ViewSettingsButton(Local("Custom Lighting...", "自定义灯光..."), SetAdvancedLighting),
                ViewSettingsButton(Local("Reset Lighting", "重置灯光"), () => ExecuteSafe(Session.Engine.ResetSceneLighting))),
            Row(EnumCombo(Local("Material", "材质"), OcctMaterial.Default, material =>
                {
                    var apply = System.Windows.MessageBox.Show(this,
                        DemoLocalization.Text("Dialog.ApplyExistingMaterial"),
                        DemoLocalization.Text("Menu.Material"),
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes;
                    ExecuteSafe(() => Session.Engine.SetDefaultMaterial(material, apply));
                    Log($"{DemoLocalization.Text("Menu.Material")}: {MaterialDisplayName(material)}");
                })),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.AutoZFit"), _autoZFitEnabled, v =>
                    ExecuteSafe(() =>
                    {
                        _autoZFitEnabled = v;
                        Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                        var msg = DemoLocalization.Text(_autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                        CommandStatus.Text = msg;
                        Log(msg);
                    })),
                ViewSettingsButton(DemoLocalization.Text("Menu.AutoZFitNow"), () => ExecuteSafe(Session.Engine.AutoZFit))),
            Row(EnumCombo(Local("Depth Bias", "深度偏移"), DemoDepthBiasPreset.Default,
                    p => ExecuteSafe(() => ApplyDepthBias(p)))));

        var window = new Window
        {
            Title = Local("View Settings", "视图设置"),
            Owner = this,
            Width = 560,
            Height = 640,
            MinWidth = 480,
            MinHeight = 400,
            Content = new Controls.ScrollViewer
            {
                VerticalScrollBarVisibility = Controls.ScrollBarVisibility.Auto,
                Content = root
            },
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
        };
        _advancedViewSettingsWindow = window;
        window.Closed += (_, _) => _advancedViewSettingsWindow = null;
        window.Show();
    }

    // ── Layout helpers ───────────────────────────────────────────────────────────

    /// <summary>One grid row: up to two cells side by side, each cell 260 px wide.</summary>
    private static Controls.Grid Row(params System.Windows.UIElement[] cells)
    {
        var grid = new Controls.Grid { Margin = new System.Windows.Thickness(0, 3, 0, 3) };
        grid.ColumnDefinitions.Add(new Controls.ColumnDefinition { Width = new System.Windows.GridLength(260) });
        grid.ColumnDefinitions.Add(new Controls.ColumnDefinition { Width = new System.Windows.GridLength(260) });
        for (var i = 0; i < cells.Length && i < 2; i++)
        {
            Controls.Grid.SetColumn(cells[i], i);
            grid.Children.Add(cells[i]);
        }
        return grid;
    }

    private static Controls.TabItem ViewSettingsTab(string text, params Controls.Grid[] rows)
    {
        var panel = new Controls.StackPanel { Margin = new System.Windows.Thickness(12) };
        foreach (var row in rows) panel.Children.Add(row);
        return new Controls.TabItem
        {
            Header = text,
            Content = new Controls.ScrollViewer { Content = panel }
        };
    }

    private static Controls.Button ViewSettingsButton(string text, Action action)
    {
        var button = new Controls.Button
        {
            Content = text,
            Height = 28,
            Margin = new System.Windows.Thickness(0, 0, 6, 0),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static Controls.CheckBox ViewSettingsCheckBox(string text, bool initialValue, Action<bool> action)
    {
        var box = new Controls.CheckBox
        {
            Content = text,
            IsChecked = initialValue,
            Height = 28,
            VerticalContentAlignment = System.Windows.VerticalAlignment.Center
        };
        box.Checked   += (_, _) => action(true);
        box.Unchecked += (_, _) => action(false);
        return box;
    }

    private static Controls.StackPanel EnumCombo<TEnum>(string label, TEnum current, Action<TEnum> apply, params TEnum[] values)
        where TEnum : struct, Enum
    {
        var panel = new Controls.StackPanel { Orientation = Controls.Orientation.Vertical, Margin = new System.Windows.Thickness(0, 0, 6, 0) };
        panel.Children.Add(new Controls.TextBlock { Text = label, Margin = new System.Windows.Thickness(0, 0, 0, 2) });
        var combo = new Controls.ComboBox { HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch };
        foreach (var v in values.Length == 0 ? Enum.GetValues<TEnum>() : values) combo.Items.Add(v);
        combo.SelectedItem = current;
        combo.SelectionChanged += (_, _) => { if (combo.SelectedItem is TEnum v) apply(v); };
        panel.Children.Add(combo);
        return panel;
    }

    private static Controls.StackPanel IntInput(string label, int initial, int min, int max, Action<int> apply)
    {
        var panel = new Controls.StackPanel { Orientation = Controls.Orientation.Vertical, Margin = new System.Windows.Thickness(0, 0, 6, 0) };
        panel.Children.Add(new Controls.TextBlock { Text = label, Margin = new System.Windows.Thickness(0, 0, 0, 2) });

        // Row: text box + up/down buttons
        var row = new Controls.StackPanel { Orientation = Controls.Orientation.Horizontal };
        var box = new Controls.TextBox
        {
            Text = initial.ToString(),
            Width = 70,
            Height = 26,
            VerticalContentAlignment = System.Windows.VerticalAlignment.Center
        };
        void TryApply()
        {
            if (int.TryParse(box.Text, out var v))
            {
                v = Math.Clamp(v, min, max);
                box.Text = v.ToString();
                apply(v);
            }
        }
        void Step(int delta)
        {
            if (int.TryParse(box.Text, out var v))
                box.Text = Math.Clamp(v + delta, min, max).ToString();
            TryApply();
        }
        box.LostFocus += (_, _) => TryApply();
        box.KeyDown   += (_, e) => { if (e.Key == System.Windows.Input.Key.Enter) TryApply(); };

        var up   = new Controls.Button { Content = "▲", Width = 22, Height = 13, Padding = new System.Windows.Thickness(0), FontSize = 8 };
        var down = new Controls.Button { Content = "▼", Width = 22, Height = 13, Padding = new System.Windows.Thickness(0), FontSize = 8 };
        up.Click   += (_, _) => Step(1);
        down.Click += (_, _) => Step(-1);

        var btns = new Controls.StackPanel { Orientation = Controls.Orientation.Vertical, Margin = new System.Windows.Thickness(2, 0, 0, 0) };
        btns.Children.Add(up);
        btns.Children.Add(down);

        row.Children.Add(box);
        row.Children.Add(btns);
        panel.Children.Add(row);
        return panel;
    }

    private void PickGradientColor(bool first)
    {
        var initial = first ? _gradientFirstColor : _gradientSecondColor;
        if (!WpfColorDialog.TryPick(this,
                first ? Local("Gradient First Color", "渐变颜色一") : Local("Gradient Second Color", "渐变颜色二"),
                initial, out var color)) return;
        if (first) _gradientFirstColor = color; else _gradientSecondColor = color;
        ApplyGradientBackground();
    }

    private void ApplyGradientBackground()
    {
        ExecuteSafe(() =>
        {
            Session.Engine.SetGradientBackground(_gradientFirstColor, _gradientSecondColor, _gradientFillMethod);
            Log($"{DemoLocalization.Text("Menu.GradientBackground")}: {_gradientFillMethod}");
        });
    }
}
