using System.Drawing;
using OcctDemo.Common;
using OcctNet;
using Controls = System.Windows.Controls;
using Window = System.Windows.Window;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private Window? _advancedViewSettingsWindow;
    private Color _gradientFirstColor = Color.White;
    private Color _gradientSecondColor = Color.LightSteelBlue;
    private OcctGradientFillMethod _gradientFillMethod = OcctGradientFillMethod.Vertical;

    private void ShowAdvancedViewSettingsWindow()
    {
        if (_advancedViewSettingsWindow is { IsVisible: true })
        {
            _advancedViewSettingsWindow.Activate();
            return;
        }

        var tabs = new Controls.TabControl();
        tabs.Items.Add(ViewSettingsTab(Local("Camera", "相机"),
            ViewSettingsButton(DemoLocalization.Text("Menu.FitAll"), () => Session.Engine.FitAll()),
            ViewSettingsButton(DemoLocalization.Text("Menu.FitSelected"), () => { var shape = ActiveShape(); if (shape is not null) Session.Engine.Fit(shape.Value); }),
            ViewSettingsButton(Local("Zoom In", "放大"), () => Session.Engine.Zoom(1.2)),
            ViewSettingsButton(Local("Zoom Out", "缩小"), () => Session.Engine.Zoom(1.0 / 1.2)),
            EnumCombo(Local("Projection", "投影"), _projectionType, SetProjectionMode),
            ViewSettingsButton(DemoLocalization.Text("Menu.PerspectiveFov"), SetPerspectiveFov),
            ViewSettingsButton(Local("Zoom Sensitivity...", "缩放灵敏度..."), SetZoomSensitivity)));

        tabs.Items.Add(ViewSettingsTab(Local("Display", "显示"),
            EnumCombo(Local("Display Style", "显示样式"), _displayMode, SetDisplayStyle),
            ViewSettingsCheckBox(DemoLocalization.Text("Menu.ShadedEdges"), true, value => Session.Engine.SetFaceBoundariesVisible(value)),
            ViewSettingsCheckBox(DemoLocalization.Text("Menu.Hlr"), false, value => Session.Engine.SetComputedHlr(value)),
            ViewSettingsCheckBox(DemoLocalization.Text("Menu.Antialiasing"), true, value => Session.Engine.SetAntialiasing(value)),
            ViewSettingsButton(DemoLocalization.Text("Menu.DisplayPrecision"), SetDisplayPrecision),
            ViewSettingsButton(DemoLocalization.Text("Menu.Background"), SetBackgroundColor),
            ViewSettingsButton(Local("Gradient First Color...", "渐变颜色一..."), () => PickGradientColor(true)),
            ViewSettingsButton(Local("Gradient Second Color...", "渐变颜色二..."), () => PickGradientColor(false)),
            EnumCombo(Local("Gradient Method", "渐变方式"), _gradientFillMethod, value => { _gradientFillMethod = value; ApplyGradientBackground(); }),
            ViewSettingsButton(DemoLocalization.Text("Menu.GradientBackground"), ApplyGradientBackground)));

        tabs.Items.Add(ViewSettingsTab(Local("Selection", "选择"),
            EnumCombo(DemoLocalization.Text("Menu.SelectionMode"), (OcctSelectionMode)0, SetSelectionMode),
            ViewSettingsCheckBox(DemoLocalization.Text("Menu.WindowSelection"), (Viewport.InteractionFeatures & OcctViewportInteractionFeatures.RectangleSelection) != 0, SetWindowSelectionEnabled),
            ViewSettingsButton(DemoLocalization.Text("Menu.SelectionTolerance"), SetSelectionTolerance),
            ViewSettingsButton(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColor),
            ViewSettingsButton(Local("Hover Color...", "悬浮高亮颜色..."), SetHoverHighlightColor),
            EnumCombo(Local("Selected Highlight Mode", "选中高亮模式"), _selectionHighlightMode, SetSelectionHighlightMode),
            EnumCombo(Local("Hover Highlight Mode", "悬浮高亮模式"), _hoverHighlightMode, SetHoverHighlightMode)));

        tabs.Items.Add(ViewSettingsTab(Local("Helpers", "辅助"),
            ViewSettingsCheckBox(DemoLocalization.Text("Menu.Triedron"), true, value => Session.Engine.SetTriedronVisible(value)),
            ViewSettingsCheckBox(DemoLocalization.Text("Menu.ViewCube"), true, value => Session.Engine.SetViewCubeVisible(value)),
            EnumCombo(Local("Triedron Position", "坐标轴位置"), _triedronPosition, SetTriedronPosition),
            EnumCombo(Local("ViewCube Position", "ViewCube 位置"), _viewCubePosition, SetViewCubePosition),
            EnumCombo(Local("ViewCube Size", "ViewCube 大小"), 90, SetViewCubeSize, 72, 90, 120),
            EnumCombo(Local("ViewCube Offset", "ViewCube 偏移"), 10, value => SetViewCubeOffset(value, value), 0, 10, 20, 40)));

        tabs.Items.Add(ViewSettingsTab(Local("Appearance", "外观"),
            BuildLightingMenu(),
            BuildMaterialMenu(),
            BuildDepthMenu()));

        var window = new Window
        {
            Title = Local("View Settings", "视图设置"),
            Owner = this,
            Width = 560,
            Height = 660,
            Content = tabs,
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
        };
        _advancedViewSettingsWindow = window;
        window.Closed += (_, _) => _advancedViewSettingsWindow = null;
        window.Show();
    }

    private static Controls.TabItem ViewSettingsTab(string text, params System.Windows.UIElement[] controls)
    {
        var panel = new Controls.StackPanel { Margin = new System.Windows.Thickness(12) };
        foreach (var control in controls)
        {
            if (control is System.Windows.FrameworkElement fe)
                fe.Margin = new System.Windows.Thickness(4);
            panel.Children.Add(control);
        }
        return new Controls.TabItem
        {
            Header = text,
            Content = new Controls.ScrollViewer { Content = panel }
        };
    }

    private static Controls.Button ViewSettingsButton(string text, Action action)
    {
        var button = new Controls.Button { Content = text, Width = 240, Height = 30, HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
        button.Click += (_, _) => action();
        return button;
    }

    private static Controls.CheckBox ViewSettingsCheckBox(string text, bool initialValue, Action<bool> action)
    {
        var box = new Controls.CheckBox { Content = text, IsChecked = initialValue, Width = 300, Height = 28 };
        box.Checked += (_, _) => action(true);
        box.Unchecked += (_, _) => action(false);
        return box;
    }

    private static System.Windows.UIElement EnumCombo<TEnum>(string label, TEnum current, Action<TEnum> apply, params TEnum[] values)
        where TEnum : struct, Enum
    {
        var panel = new Controls.StackPanel { Orientation = Controls.Orientation.Horizontal, Width = 500 };
        panel.Children.Add(new Controls.TextBlock { Text = label, Width = 170, VerticalAlignment = System.Windows.VerticalAlignment.Center });
        var combo = new Controls.ComboBox { Width = 220 };
        foreach (var value in values.Length == 0 ? Enum.GetValues<TEnum>() : values) combo.Items.Add(value);
        combo.SelectedItem = current;
        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is TEnum value) apply(value);
        };
        panel.Children.Add(combo);
        return panel;
    }

    private static System.Windows.UIElement EnumCombo(string label, int current, Action<int> apply, params int[] values)
    {
        var panel = new Controls.StackPanel { Orientation = Controls.Orientation.Horizontal, Width = 500 };
        panel.Children.Add(new Controls.TextBlock { Text = label, Width = 170, VerticalAlignment = System.Windows.VerticalAlignment.Center });
        var combo = new Controls.ComboBox { Width = 220 };
        foreach (var value in values) combo.Items.Add(value);
        combo.SelectedItem = current;
        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is int value) apply(value);
        };
        panel.Children.Add(combo);
        return panel;
    }

    private void PickGradientColor(bool first)
    {
        var initial = first ? _gradientFirstColor : _gradientSecondColor;
        if (!WpfColorDialog.TryPick(this, first ? Local("Gradient First Color", "渐变颜色一") : Local("Gradient Second Color", "渐变颜色二"), initial, out var color)) return;
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
