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
    private DemoVisualStyle _visualStyle = DemoVisualStyle.ShadedEdges;

    private enum DemoVisualStyle
    {
        Wireframe,
        Shaded,
        ShadedEdges,
        HiddenLine
    }

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
            Size = new Size(560, 640),
            MinimumSize = new Size(480, 400),
            MinimizeBox = true,
            MaximizeBox = true,
            ShowInTaskbar = false
        };

        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(12, 8, 12, 12)
        };

        var root = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Padding = new Padding(0),
            Width = 520
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void AddSection(string title, params Control[] rows)
        {
            root.Controls.Add(SectionHeader(title));
            foreach (var row in rows)
                root.Controls.Add(row);
            root.Controls.Add(new Panel { Height = 8, Width = 1 });
        }

        // ── Display ──
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

        // ── Camera ──
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

        // ── Selection ──
        AddSection(Local("Selection", "选择"),
            Row(EnumCombo(DemoLocalization.Text("Menu.SelectionMode"),
                    (OcctSelectionMode)Math.Max(_selectionCombo.SelectedIndex, 0), SetSelectionMode)),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.WindowSelection"),
                    (_viewport.InteractionFeatures & OcctViewportInteractionFeatures.RectangleSelection) != 0,
                    SetWindowSelectionEnabled),
                ViewSettingsButton(DemoLocalization.Text("Menu.SelectionTolerance"), SetSelectionTolerance)),
            Row(ViewSettingsButton(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColor),
                ViewSettingsButton(Local("Hover Color...", "悬浮高亮颜色..."), SetHoverHighlightColor)),
            Row(EnumCombo(Local("Selected Highlight Mode", "选中高亮模式"), _selectionHighlightMode, SetSelectionHighlightMode),
                EnumCombo(Local("Hover Highlight Mode", "悬浮高亮模式"), _hoverHighlightMode, SetHoverHighlightMode)));

        // ── Helpers / ViewCube ──
        AddSection(Local("Helpers", "辅助"),
            Row(ViewSettingsCheckBox(DemoLocalization.Text("Menu.Triedron"), true, v => Session.Engine.SetTriedronVisible(v)),
                ViewSettingsCheckBox(DemoLocalization.Text("Menu.ViewCube"), true, v => SetViewCubeVisible(v))),
            Row(EnumCombo(Local("Triedron Position", "坐标轴位置"), _triedronPosition, SetTriedronPosition),
                EnumCombo(Local("ViewCube Position", "ViewCube 位置"), _viewCubePosition, SetViewCubePosition)),
            Row(IntInput(Local("ViewCube Size (px)", "ViewCube 大小(px)"), _viewCubeSize, 10, 300, SetViewCubeSize),
                IntInput(Local("ViewCube Font Height", "ViewCube 文字大小"), _viewCubeFontHeight, 8, 48, SetViewCubeFontHeight)),
            Row(IntInput(Local("ViewCube Offset X (px)", "ViewCube 偏移 X(px)"), _viewCubeOffsetX, 0, 200, SetViewCubeOffsetX),
                IntInput(Local("ViewCube Offset Y (px)", "ViewCube 偏移 Y(px)"), _viewCubeOffsetY, 0, 200, SetViewCubeOffsetY)),
                        Row(ViewSettingsButton(Local("ViewCube Face Color...", "ViewCube 面颜色..."), PickViewCubeBoxColor),
                ViewSettingsButton(Local("ViewCube Highlight Color...", "ViewCube 高亮颜色..."), PickViewCubeFacetColor)),
            Row(ViewSettingsButton(Local("ViewCube Text Color...", "ViewCube 文字颜色..."), PickViewCubeTextColor)));

        // ── Appearance ──
        AddSection(Local("Appearance", "外观"),
            Row(EnumCombo(Local("Lighting Preset", "灯光预设"), OcctLightingPreset.Neutral,
                    p => ExecuteSafe(() => ApplyLightingPreset(p)))),
            Row(ViewSettingsButton(Local("Custom Lighting...", "自定义灯光..."), SetAdvancedLighting),
                ViewSettingsButton(Local("Reset Lighting", "重置灯光"), () => ExecuteSafe(Session.Engine.ResetSceneLighting))),
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
                    p => ExecuteSafe(() => ApplyDepthBias(p)))));

        scroll.Controls.Add(root);
        window.Controls.Add(scroll);
        _advancedViewSettingsWindow = window;
        window.FormClosed += (_, _) => _advancedViewSettingsWindow = null;
        window.Show(this);
    }

    private void ApplyVisualStyle(DemoVisualStyle style)
    {
        _visualStyle = style;
        ExecuteSafe(() =>
        {
            switch (style)
            {
                case DemoVisualStyle.Wireframe:
                    _displayMode = OcctDisplayMode.Wireframe;
                    Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe);
                    Session.Engine.SetComputedHlr(false);
                    break;
                case DemoVisualStyle.Shaded:
                    _displayMode = OcctDisplayMode.Shaded;
                    Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded);
                    Session.Engine.SetFaceBoundariesVisible(false);
                    Session.Engine.SetComputedHlr(false);
                    break;
                case DemoVisualStyle.ShadedEdges:
                    _displayMode = OcctDisplayMode.Shaded;
                    Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded);
                    Session.Engine.SetFaceBoundariesVisible(true);
                    Session.Engine.SetComputedHlr(false);
                    break;
                case DemoVisualStyle.HiddenLine:
                    _displayMode = OcctDisplayMode.Wireframe;
                    Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe);
                    Session.Engine.SetComputedHlr(true);
                    break;
            }
            var name = style switch
            {
                DemoVisualStyle.Wireframe => Local("Wireframe", "线框"),
                DemoVisualStyle.Shaded => Local("Shaded", "着色"),
                DemoVisualStyle.ShadedEdges => Local("Shaded + Edges", "着色+边线"),
                _ => Local("Hidden Line Removal", "消隐模式")
            };
            Log($"{Local("Display Style", "显示样式")}: {name}");
        });
    }

    private static Label SectionHeader(string text) =>
        new()
        {
            Text = text,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Padding = new Padding(0, 6, 0, 4),
            Margin = new Padding(0, 4, 0, 2)
        };

    private const int ColWidth = 248;

    private static TableLayoutPanel Row(params Control[] cells)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0),
            Margin = new Padding(0, 2, 0, 2),
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
        var p = new Panel { Height = 48, AutoSize = false };
        var lbl = new Label { Text = label, Left = 0, Top = 0, Width = ColWidth - 4, Height = 16 };
        var cb = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Left = 0, Top = 18, Width = ColWidth - 8, Height = 24
        };
        var list = values.Length == 0 ? Enum.GetValues<TEnum>() : values;
        foreach (var v in list)
            cb.Items.Add(FormatEnumLabel(v));
        var idx = Array.IndexOf(list.ToArray(), current);
        cb.SelectedIndex = idx >= 0 ? idx : 0;
        cb.SelectedIndexChanged += (_, _) =>
        {
            if (cb.SelectedIndex >= 0 && cb.SelectedIndex < list.Length)
                apply(list.ElementAt(cb.SelectedIndex));
        };
        p.Controls.AddRange(new Control[] { lbl, cb });
        return p;
    }

    private static string FormatEnumLabel<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        if (value is DemoVisualStyle vs)
        {
            return vs switch
            {
                DemoVisualStyle.Wireframe => DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "线框" : "Wireframe",
                DemoVisualStyle.Shaded => DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "着色" : "Shaded",
                DemoVisualStyle.ShadedEdges => DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "着色+边线" : "Shaded + Edges",
                DemoVisualStyle.HiddenLine => DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "消隐模式" : "Hidden Line Removal",
                _ => value.ToString() ?? ""
            };
        }
        return value.ToString() ?? "";
    }

    private static Panel IntInput(string label, int initial, int min, int max, Action<int> apply)
    {
        var p = new Panel { Height = 48, AutoSize = false };
        var lbl = new Label { Text = label, Left = 0, Top = 0, Width = ColWidth - 4, Height = 16 };
        var nud = new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            Value = Math.Clamp(initial, min, max),
            Left = 0,
            Top = 18,
            Width = ColWidth - 8,
            Height = 26,
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
