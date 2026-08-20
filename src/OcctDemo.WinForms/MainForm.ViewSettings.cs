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
            Size = new Size(580, 680),
            MinimizeBox = true,
            MaximizeBox = false,
            ShowInTaskbar = false
        };

        var tabs = new TabControl { Dock = DockStyle.Fill };

        // ── Camera tab ────────────────────────────────────────────────────────────
        tabs.TabPages.Add(ViewSettingsTab(Local("Camera", "相机"),
            Row(ViewSettingsButton(DemoLocalization.Text("Menu.FitAll"),     () => Session.Engine.FitAll()),
                ViewSettingsButton(DemoLocalization.Text("Menu.FitSelected"),() => { var s = ActiveShape(); if (s is not null) Session.Engine.Fit(s.Value); })),
            Row(ViewSettingsButton(Local("Zoom In",  "放大"), () => Session.Engine.Zoom(1.2)),
                ViewSettingsButton(Local("Zoom Out", "缩小"), () => Session.Engine.Zoom(1.0 / 1.2))),
            Row(EnumCombo(Local("Projection", "投影"), _projectionType, SetProjectionMode)),
            Row(ViewSettingsButton(DemoLocalization.Text("Menu.PerspectiveFov"),     SetPerspectiveFov),
                ViewSettingsButton(Local("Zoom Sensitivity...", "缩放灵敏度..."),     SetZoomSensitivity))));

        // ── Display tab ───────────────────────────────────────────────────────────
        tabs.TabPages.Add(ViewSettingsTab(Local("Display", "显示"),
            Row(EnumCombo(Local("Display Style", "显示样式"), _displayMode, SetDisplayStyle)),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.ShadedEdges"),  true,  v => Session.Engine.SetFaceBoundariesVisible(v)),
                ViewSettingsCheckBox(DemoLocalization.Text("Menu.Hlr"),          false, v => Session.Engine.SetComputedHlr(v))),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.Antialiasing"), true,  v => Session.Engine.SetAntialiasing(v))),
            Row(ViewSettingsButton(DemoLocalization.Text("Menu.DisplayPrecision"), SetDisplayPrecision)),
            Row(ViewSettingsButton(DemoLocalization.Text("Menu.Background"),       SetBackgroundColor),
                ViewSettingsButton(Local("Gradient First Color...", "渐变颜色一..."),  () => PickGradientColor(true))),
            Row(ViewSettingsButton(Local("Gradient Second Color...", "渐变颜色二..."), () => PickGradientColor(false)),
                ViewSettingsButton(DemoLocalization.Text("Menu.GradientBackground"),   ApplyGradientBackground)),
            Row(EnumCombo(Local("Gradient Method", "渐变方式"), _gradientFillMethod,
                    v => { _gradientFillMethod = v; ApplyGradientBackground(); }))));

        // ── Selection tab ─────────────────────────────────────────────────────────
        tabs.TabPages.Add(ViewSettingsTab(Local("Selection", "选择"),
            Row(EnumCombo(DemoLocalization.Text("Menu.SelectionMode"),
                    (OcctSelectionMode)Math.Max(_selectionCombo.SelectedIndex, 0), SetSelectionMode)),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.WindowSelection"),
                    (_viewport.InteractionFeatures & OcctViewportInteractionFeatures.RectangleSelection) != 0,
                    SetWindowSelectionEnabled)),
            Row(ViewSettingsButton(DemoLocalization.Text("Menu.SelectionTolerance"), SetSelectionTolerance)),
            Row(ViewSettingsButton(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColor),
                ViewSettingsButton(Local("Hover Color...", "悬浮高亮颜色..."),   SetHoverHighlightColor)),
            Row(EnumCombo(Local("Selected Highlight Mode", "选中高亮模式"), _selectionHighlightMode, SetSelectionHighlightMode)),
            Row(EnumCombo(Local("Hover Highlight Mode",    "悬浮高亮模式"), _hoverHighlightMode,    SetHoverHighlightMode))));

        // ── Helpers tab ───────────────────────────────────────────────────────────
        tabs.TabPages.Add(ViewSettingsTab(Local("Helpers", "辅助"),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.Triedron"), true, v => Session.Engine.SetTriedronVisible(v)),
                ViewSettingsCheckBox(DemoLocalization.Text("Menu.ViewCube"), true, v => SetViewCubeVisible(v))),
            Row(EnumCombo(Local("Triedron Position",  "坐标轴位置"),    _triedronPosition,  SetTriedronPosition)),
            Row(EnumCombo(Local("ViewCube Position",  "ViewCube 位置"), _viewCubePosition,  SetViewCubePosition)),
            Row(IntInput(Local("ViewCube Size (px)",  "ViewCube 大小(px)"), _viewCubeSize,  10, 300, SetViewCubeSize),
                IntInput(Local("ViewCube Offset (px)","ViewCube 偏移(px)"), _viewCubeOffset, 0, 200, v => SetViewCubeOffset(v, v)))));

        // ── Appearance tab ────────────────────────────────────────────────────────
        tabs.TabPages.Add(ViewSettingsTab(Local("Appearance", "外观"),
            Row(EnumCombo(Local("Lighting Preset", "灯光预设"), OcctLightingPreset.Studio,
                    p => ExecuteSafe(() => ApplyLightingPreset(p)))),
            Row(ViewSettingsButton(Local("Custom Lighting...", "自定义灯光..."),  SetAdvancedLighting),
                ViewSettingsButton(Local("Reset Lighting",     "重置灯光"),       () => ExecuteSafe(Session.Engine.ResetSceneLighting))),
            Row(EnumCombo(Local("Material", "材质"), OcctMaterial.Default, material =>
                {
                    var apply = MessageBox.Show(this,
                        DemoLocalization.Text("Dialog.ApplyExistingMaterial"),
                        DemoLocalization.Text("Menu.Material"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
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
            Row(EnumCombo(Local("Depth Bias", "深度偏移"), DemoDepthBiasPreset.Default,
                    p => ExecuteSafe(() => ApplyDepthBias(p))))));

        window.Controls.Add(tabs);
        _advancedViewSettingsWindow = window;
        window.FormClosed += (_, _) => _advancedViewSettingsWindow = null;
        window.Show(this);
    }

    // ── Layout helpers ────────────────────────────────────────────────────────────

    private const int ColWidth = 256;

    private static TableLayoutPanel Row(params Control[] cells)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0),
            Margin = new Padding(0, 3, 0, 3),
            Width = ColWidth * 2 + 8
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ColWidth));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ColWidth));
        for (var i = 0; i < cells.Length && i < 2; i++)
        {
            cells[i].Dock = DockStyle.Fill;
            cells[i].Margin = i == 0 ? new Padding(0, 0, 4, 0) : new Padding(4, 0, 0, 0);
            row.Controls.Add(cells[i], i, 0);
        }
        return row;
    }

    private static TabPage ViewSettingsTab(string text, params TableLayoutPanel[] rows)
    {
        var page  = new TabPage(text);
        var panel = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            AutoScroll    = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents  = false,
            Padding       = new Padding(12)
        };
        foreach (var row in rows) panel.Controls.Add(row);
        page.Controls.Add(panel);
        return page;
    }

    private static Button ViewSettingsButton(string text, Action action)
    {
        var b = new Button { Text = text, Height = 28, AutoSize = false };
        b.Click += (_, _) => action();
        return b;
    }

    private static CheckBox ViewSettingsCheckBox(string text, bool initial, Action<bool> action)
    {
        var cb = new CheckBox { Text = text, Checked = initial, Height = 28, AutoSize = false };
        cb.CheckedChanged += (_, _) => action(cb.Checked);
        return cb;
    }

    private static Panel EnumCombo<TEnum>(string label, TEnum current, Action<TEnum> apply, params TEnum[] values)
        where TEnum : struct, Enum
    {
        var p = new Panel { Height = 50, AutoSize = false };
        var lbl = new Label { Text = label, Left = 0, Top = 0, Width = ColWidth - 4, Height = 18 };
        var cb  = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Left = 0, Top = 20, Width = ColWidth - 8, Height = 24
        };
        foreach (var v in values.Length == 0 ? Enum.GetValues<TEnum>() : values) cb.Items.Add(v);
        cb.SelectedItem = current;
        cb.SelectedIndexChanged += (_, _) => { if (cb.SelectedItem is TEnum v) apply(v); };
        p.Controls.AddRange(new Control[] { lbl, cb });
        return p;
    }

    private static Panel IntInput(string label, int initial, int min, int max, Action<int> apply)
    {
        var p   = new Panel { Height = 50, AutoSize = false };
        var lbl = new Label { Text = label, Left = 0, Top = 0, Width = ColWidth - 4, Height = 18 };
        var nud = new NumericUpDown
        {
            Minimum  = min,
            Maximum  = max,
            Value    = Math.Clamp(initial, min, max),
            Left     = 0,
            Top      = 20,
            Width    = ColWidth - 8,
            Height   = 26,
            DecimalPlaces = 0
        };
        nud.ValueChanged += (_, _) => apply((int)nud.Value);
        p.Controls.AddRange(new Control[] { lbl, nud });
        return p;
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
