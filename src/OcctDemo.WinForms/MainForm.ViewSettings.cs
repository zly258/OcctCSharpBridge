using System.Drawing;
using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    private Form? _advancedViewSettingsWindow;
    private Color _gradientFirstColor = Color.White;
    private Color _gradientSecondColor = Color.LightSteelBlue;
    private OcctGradientFillMethod _gradientFillMethod = OcctGradientFillMethod.Vertical;

    private void ShowAdvancedViewSettingsWindow()
    {
        if (_advancedViewSettingsWindow is { IsDisposed: false })
        {
            _advancedViewSettingsWindow.Activate();
            return;
        }

        var window = new Form
        {
            Text = Local("View Settings", "视图设置"),
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(560, 660),
            MinimizeBox = true,
            MaximizeBox = false,
            ShowInTaskbar = false
        };

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(ViewSettingsTab(Local("Camera", "相机"),
            ViewSettingsButton(DemoLocalization.Text("Menu.FitAll"), () => Session.Engine.FitAll()),
            ViewSettingsButton(DemoLocalization.Text("Menu.FitSelected"), () => { var shape = ActiveShape(); if (shape is not null) Session.Engine.Fit(shape.Value); }),
            ViewSettingsButton(Local("Zoom In", "放大"), () => Session.Engine.Zoom(1.2)),
            ViewSettingsButton(Local("Zoom Out", "缩小"), () => Session.Engine.Zoom(1.0 / 1.2)),
            EnumCombo(Local("Projection", "投影"), _projectionType, SetProjectionMode),
            ViewSettingsButton(DemoLocalization.Text("Menu.PerspectiveFov"), SetPerspectiveFov),
            ViewSettingsButton(Local("Zoom Sensitivity...", "缩放灵敏度..."), SetZoomSensitivity)));

        tabs.TabPages.Add(ViewSettingsTab(Local("Display", "显示"),
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

        tabs.TabPages.Add(ViewSettingsTab(Local("Selection", "选择"),
            EnumCombo(DemoLocalization.Text("Menu.SelectionMode"), (OcctSelectionMode)Math.Max(_selectionCombo.SelectedIndex, 0), SetSelectionMode),
            ViewSettingsCheckBox(DemoLocalization.Text("Menu.WindowSelection"), (_viewport.InteractionFeatures & OcctViewportInteractionFeatures.RectangleSelection) != 0, SetWindowSelectionEnabled),
            ViewSettingsButton(DemoLocalization.Text("Menu.SelectionTolerance"), SetSelectionTolerance),
            ViewSettingsButton(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColor),
            ViewSettingsButton(Local("Hover Color...", "悬浮高亮颜色..."), SetHoverHighlightColor),
            EnumCombo(Local("Selected Highlight Mode", "选中高亮模式"), _selectionHighlightMode, SetSelectionHighlightMode),
            EnumCombo(Local("Hover Highlight Mode", "悬浮高亮模式"), _hoverHighlightMode, SetHoverHighlightMode)));

        tabs.TabPages.Add(ViewSettingsTab(Local("Helpers", "辅助"),
            ViewSettingsCheckBox(DemoLocalization.Text("Menu.Triedron"), true, value => Session.Engine.SetTriedronVisible(value)),
            ViewSettingsCheckBox(DemoLocalization.Text("Menu.ViewCube"), true, value => Session.Engine.SetViewCubeVisible(value)),
            EnumCombo(Local("Triedron Position", "坐标轴位置"), _triedronPosition, SetTriedronPosition),
            EnumCombo(Local("ViewCube Position", "ViewCube 位置"), _viewCubePosition, SetViewCubePosition),
            ValueCombo(Local("ViewCube Size", "ViewCube 大小"), 90, SetViewCubeSize, 72, 90, 120),
            ValueCombo(Local("ViewCube Offset", "ViewCube 偏移"), 10, value => SetViewCubeOffset(value, value), 0, 10, 20, 40)));

        tabs.TabPages.Add(ViewSettingsTab(Local("Appearance", "外观"),
            MenuHost(Local("Lighting", "灯光"), BuildLightingMenu()),
            MenuHost(Local("Material", "材质"), BuildMaterialMenu()),
            MenuHost(Local("Depth", "深度处理"), BuildDepthMenu())));

        window.Controls.Add(tabs);
        _advancedViewSettingsWindow = window;
        window.FormClosed += (_, _) => _advancedViewSettingsWindow = null;
        window.Show(this);
    }

    private static TabPage ViewSettingsTab(string text, params Control[] controls)
    {
        var page = new TabPage(text);
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12)
        };
        foreach (var control in controls) panel.Controls.Add(control);
        page.Controls.Add(panel);
        return page;
    }

    private static Button ViewSettingsButton(string text, Action action)
    {
        var button = new Button { Text = text, Width = 240, Height = 30, Margin = new Padding(4) };
        button.Click += (_, _) => action();
        return button;
    }

    private static CheckBox ViewSettingsCheckBox(string text, bool initialValue, Action<bool> action)
    {
        var box = new CheckBox { Text = text, Checked = initialValue, Width = 280, Height = 28, Margin = new Padding(4) };
        box.CheckedChanged += (_, _) => action(box.Checked);
        return box;
    }

    private static Control EnumCombo<TEnum>(string label, TEnum current, Action<TEnum> apply, params TEnum[] values)
        where TEnum : struct, Enum
    {
        var panel = new FlowLayoutPanel { Width = 500, Height = 34, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        panel.Controls.Add(new Label { Text = label, Width = 170, Height = 28, TextAlign = ContentAlignment.MiddleLeft });
        var combo = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var value in values.Length == 0 ? Enum.GetValues<TEnum>() : values) combo.Items.Add(value);
        combo.SelectedItem = current;
        combo.SelectedIndexChanged += (_, _) =>
        {
            if (combo.SelectedItem is TEnum value) apply(value);
        };
        panel.Controls.Add(combo);
        return panel;
    }

    private static Control ValueCombo(string label, int current, Action<int> apply, params int[] values)
    {
        var panel = new FlowLayoutPanel { Width = 500, Height = 34, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        panel.Controls.Add(new Label { Text = label, Width = 170, Height = 28, TextAlign = ContentAlignment.MiddleLeft });
        var combo = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var value in values) combo.Items.Add(value);
        combo.SelectedItem = current;
        combo.SelectedIndexChanged += (_, _) =>
        {
            if (combo.SelectedItem is int value) apply(value);
        };
        panel.Controls.Add(combo);
        return panel;
    }

    private static Control MenuHost(string text, ToolStripMenuItem sourceMenu)
    {
        var strip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            AutoSize = false,
            Width = 300,
            Height = 32,
            Margin = new Padding(4)
        };
        var button = new ToolStripDropDownButton(text) { AutoSize = false, Width = 240 };
        while (sourceMenu.DropDownItems.Count > 0)
        {
            var item = sourceMenu.DropDownItems[0];
            sourceMenu.DropDownItems.RemoveAt(0);
            button.DropDownItems.Add(item);
        }
        strip.Items.Add(button);
        return strip;
    }

    private void PickGradientColor(bool first)
    {
        using var dialog = new ColorDialog
        {
            FullOpen = true,
            Color = first ? _gradientFirstColor : _gradientSecondColor
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (first) _gradientFirstColor = dialog.Color; else _gradientSecondColor = dialog.Color;
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
