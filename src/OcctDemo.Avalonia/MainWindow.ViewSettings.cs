using Avalonia;
using Avalonia.Controls;
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

        var tabs = new TabControl
        {
            ItemsSource = new object[]
            {
                ViewSettingsTab(Local("Camera", "相机"),
                    ViewSettingsButton(DemoLocalization.Text("Menu.FitAll"), () => Session.Engine.FitAll()),
                    ViewSettingsButton(DemoLocalization.Text("Menu.FitSelected"), () => { var shape = ActiveShape(); if (shape is not null) Session.Engine.Fit(shape.Value); }),
                    ViewSettingsButton(Local("Zoom In", "放大"), () => Session.Engine.Zoom(1.2)),
                    ViewSettingsButton(Local("Zoom Out", "缩小"), () => Session.Engine.Zoom(1.0 / 1.2)),
                    EnumCombo(Local("Projection", "投影"), _projectionType, SetProjectionMode),
                    AsyncViewSettingsButton(DemoLocalization.Text("Menu.PerspectiveFov"), SetPerspectiveFovAsync),
                    AsyncViewSettingsButton(Local("Zoom Sensitivity...", "缩放灵敏度..."), SetZoomSensitivityAsync)),
                ViewSettingsTab(Local("Display", "显示"),
                    EnumCombo(Local("Display Style", "显示样式"), _displayMode, SetDisplayStyle),
                    ViewSettingsCheckBox(DemoLocalization.Text("Menu.ShadedEdges"), true, value => Session.Engine.SetFaceBoundariesVisible(value)),
                    ViewSettingsCheckBox(DemoLocalization.Text("Menu.Hlr"), false, value => Session.Engine.SetComputedHlr(value)),
                    ViewSettingsCheckBox(DemoLocalization.Text("Menu.Antialiasing"), true, value => Session.Engine.SetAntialiasing(value)),
                    AsyncViewSettingsButton(DemoLocalization.Text("Menu.DisplayPrecision"), SetDisplayPrecisionAsync),
                    AsyncViewSettingsButton(DemoLocalization.Text("Menu.Background"), SetBackgroundColorAsync),
                    AsyncViewSettingsButton(Local("Gradient First Color...", "渐变颜色一..."), () => PickGradientColorAsync(true)),
                    AsyncViewSettingsButton(Local("Gradient Second Color...", "渐变颜色二..."), () => PickGradientColorAsync(false)),
                    EnumCombo(Local("Gradient Method", "渐变方式"), _gradientFillMethod, value => { _gradientFillMethod = value; ApplyGradientBackground(); }),
                    ViewSettingsButton(DemoLocalization.Text("Menu.GradientBackground"), ApplyGradientBackground)),
                ViewSettingsTab(Local("Selection", "选择"),
                    EnumCombo(DemoLocalization.Text("Menu.SelectionMode"), (OcctSelectionMode)0, SetSelectionMode),
                    ViewSettingsCheckBox(DemoLocalization.Text("Menu.WindowSelection"), (_viewport.InteractionFeatures & OcctViewportInteractionFeatures.RectangleSelection) != 0, SetWindowSelectionEnabled),
                    AsyncViewSettingsButton(DemoLocalization.Text("Menu.SelectionTolerance"), SetSelectionToleranceAsync),
                    AsyncViewSettingsButton(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColorAsync),
                    AsyncViewSettingsButton(Local("Hover Color...", "悬浮高亮颜色..."), SetHoverHighlightColorAsync),
                    EnumCombo(Local("Selected Highlight Mode", "选中高亮模式"), _selectionHighlightMode, SetSelectionHighlightMode),
                    EnumCombo(Local("Hover Highlight Mode", "悬浮高亮模式"), _hoverHighlightMode, SetHoverHighlightMode)),
                ViewSettingsTab(Local("Helpers", "辅助"),
                    ViewSettingsCheckBox(DemoLocalization.Text("Menu.Triedron"), true, value => Session.Engine.SetTriedronVisible(value)),
                    ViewSettingsCheckBox(DemoLocalization.Text("Menu.ViewCube"), true, value => Session.Engine.SetViewCubeVisible(value)),
                    EnumCombo(Local("Triedron Position", "坐标轴位置"), _triedronPosition, SetTriedronPosition),
                    EnumCombo(Local("ViewCube Position", "ViewCube 位置"), _viewCubePosition, SetViewCubePosition),
                    EnumCombo(Local("ViewCube Size", "ViewCube 大小"), 90, SetViewCubeSize, 72, 90, 120),
                    EnumCombo(Local("ViewCube Offset", "ViewCube 偏移"), 10, value => SetViewCubeOffset(value, value), 0, 10, 20, 40)),
                ViewSettingsTab(Local("Appearance", "外观"),
                    BuildLightingMenu(),
                    BuildMaterialMenu(),
                    BuildDepthMenu())
            }
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

    private static TabItem ViewSettingsTab(string text, params Control[] controls)
    {
        var panel = new StackPanel { Margin = new Thickness(12), Spacing = 6 };
        foreach (var control in controls) panel.Children.Add(control);
        return new TabItem
        {
            Header = text,
            Content = new ScrollViewer { Content = panel }
        };
    }

    private static Button ViewSettingsButton(string text, Action action)
    {
        var button = new Button { Content = text, Width = 240, Height = 30, HorizontalAlignment = HorizontalAlignment.Left };
        button.Click += (_, _) => action();
        return button;
    }

    private static Button AsyncViewSettingsButton(string text, Func<Task> action)
    {
        var button = new Button { Content = text, Width = 240, Height = 30, HorizontalAlignment = HorizontalAlignment.Left };
        button.Click += async (_, _) => await action();
        return button;
    }

    private static CheckBox ViewSettingsCheckBox(string text, bool initialValue, Action<bool> action)
    {
        var box = new CheckBox { Content = text, IsChecked = initialValue, Width = 300, Height = 28 };
        box.IsCheckedChanged += (_, _) => action(box.IsChecked == true);
        return box;
    }

    private static Control EnumCombo<TEnum>(string label, TEnum current, Action<TEnum> apply, params TEnum[] values)
        where TEnum : struct, Enum
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Width = 500, Spacing = 8 };
        panel.Children.Add(new TextBlock { Text = label, Width = 170, VerticalAlignment = VerticalAlignment.Center });
        var combo = new ComboBox { Width = 220, ItemsSource = values.Length == 0 ? Enum.GetValues<TEnum>() : values };
        combo.SelectedItem = current;
        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is TEnum value) apply(value);
        };
        panel.Children.Add(combo);
        return panel;
    }

    private static Control EnumCombo(string label, int current, Action<int> apply, params int[] values)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Width = 500, Spacing = 8 };
        panel.Children.Add(new TextBlock { Text = label, Width = 170, VerticalAlignment = VerticalAlignment.Center });
        var combo = new ComboBox { Width = 220, ItemsSource = values };
        combo.SelectedItem = current;
        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is int value) apply(value);
        };
        panel.Children.Add(combo);
        return panel;
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
