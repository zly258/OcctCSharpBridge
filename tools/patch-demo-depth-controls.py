from pathlib import Path


def replace_exact(path: str, old: str, new: str, label: str) -> None:
    file = Path(path)
    text = file.read_text(encoding="utf-8-sig")
    if old in text:
        file.write_text(text.replace(old, new), encoding="utf-8", newline="\n")
        print(f"Updated {label}.")
    elif new in text:
        print(f"{label} already updated.")
    else:
        raise SystemExit(f"Expected anchor for {label} was not found in {path}.")


# Shared Demo session: one implementation for WinForms and WPF.
replace_exact(
    "src/CadCommon/CadSession.cs",
    "public enum CadIsoView { NorthEast, NorthWest, SouthEast, SouthWest }\n\npublic sealed class CadSession\n",
    "public enum CadIsoView { NorthEast, NorthWest, SouthEast, SouthWest }\n"
    "public enum CadDepthBiasPreset { Forward, Backward, Default }\n\n"
    "public sealed class CadSession\n",
    "shared depth-bias preset",
)

replace_exact(
    "src/CadCommon/CadSession.cs",
    "    public event EventHandler? ModelChanged;\n"
    "    public event EventHandler? HistoryChanged;\n"
    "    public event EventHandler<string>? StatusChanged;\n\n"
    "    public CadCommandResult Execute(",
    "    public event EventHandler? ModelChanged;\n"
    "    public event EventHandler? HistoryChanged;\n"
    "    public event EventHandler<string>? StatusChanged;\n\n"
    "    public int ApplyDepthBiasToSelection(CadDepthBiasPreset preset)\n"
    "    {\n"
    "        var targets = Engine.SelectedObjects\n"
    "            .Where(value => value.Kind == OcctObjectKind.Shape)\n"
    "            .DistinctBy(value => value.Id)\n"
    "            .ToList();\n\n"
    "        if (targets.Count == 0\n"
    "            && ActiveObject is { Kind: OcctObjectKind.Shape } active\n"
    "            && Engine.Exists(active))\n"
    "        {\n"
    "            targets.Add(active);\n"
    "        }\n\n"
    "        if (targets.Count == 0) return 0;\n\n"
    "        using (Engine.BeginDisplayBatch())\n"
    "        {\n"
    "            foreach (var target in targets)\n"
    "            {\n"
    "                switch (preset)\n"
    "                {\n"
    "                    case CadDepthBiasPreset.Forward:\n"
    "                        Engine.SetPolygonOffsets(\n"
    "                            target,\n"
    "                            OcctPolygonOffsetMode.Fill,\n"
    "                            factor: -1.0,\n"
    "                            units: -1.0);\n"
    "                        break;\n"
    "                    case CadDepthBiasPreset.Backward:\n"
    "                        Engine.SetPolygonOffsets(\n"
    "                            target,\n"
    "                            OcctPolygonOffsetMode.Fill,\n"
    "                            factor: 3.0,\n"
    "                            units: 3.0);\n"
    "                        break;\n"
    "                    default:\n"
    "                        Engine.ResetPolygonOffsets(target);\n"
    "                        break;\n"
    "                }\n"
    "            }\n"
    "        }\n\n"
    "        return targets.Count;\n"
    "    }\n\n"
    "    public CadCommandResult Execute(",
    "shared selected-object depth bias",
)

# Localization.
replace_exact(
    "src/CadCommon/CadLocalization.cs",
    '            ["Menu.GradientBackground"] = "Gradient Background",\n',
    '            ["Menu.GradientBackground"] = "Gradient Background",\n'
    '            ["Menu.DepthHandling"] = "Depth and Coplanar Display",\n'
    '            ["Menu.AutoZFit"] = "Automatic Z Range",\n'
    '            ["Menu.AutoZFitNow"] = "Recalculate Z Range",\n'
    '            ["Menu.DepthForward"] = "Bring Selected Forward",\n'
    '            ["Menu.DepthBackward"] = "Push Selected Back",\n'
    '            ["Menu.DepthReset"] = "Reset Selected Depth Bias",\n',
    "English depth menu localization",
)

replace_exact(
    "src/CadCommon/CadLocalization.cs",
    '            ["Status.WindowSelectionOff"] = "Window selection disabled",\n',
    '            ["Status.WindowSelectionOff"] = "Window selection disabled",\n'
    '            ["Status.AutoZFitOn"] = "Automatic Z-range fitting enabled",\n'
    '            ["Status.AutoZFitOff"] = "Automatic Z-range fitting disabled",\n'
    '            ["Status.DepthBiasApplied"] = "Depth bias updated for {0} object(s).",\n'
    '            ["Status.DepthBiasNoShape"] = "Select one or more shapes first.",\n',
    "English depth status localization",
)

replace_exact(
    "src/CadCommon/CadLocalization.cs",
    '["Menu.GradientBackground"] = "渐变背景", ["Menu.MouseHelp"] = "鼠标操作"',
    '["Menu.GradientBackground"] = "渐变背景", ["Menu.DepthHandling"] = "深度与共面显示", '
    '["Menu.AutoZFit"] = "自动 Z 范围", ["Menu.AutoZFitNow"] = "重新计算 Z 范围", '
    '["Menu.DepthForward"] = "将选中对象前移", ["Menu.DepthBackward"] = "将选中对象后移", '
    '["Menu.DepthReset"] = "恢复选中对象深度偏移", ["Menu.MouseHelp"] = "鼠标操作"',
    "Chinese depth menu localization",
)

replace_exact(
    "src/CadCommon/CadLocalization.cs",
    '["Status.WindowSelectionOff"] = "框选已关闭", ["Dialog.OpenTitle"] = "打开图形"',
    '["Status.WindowSelectionOff"] = "框选已关闭", ["Status.AutoZFitOn"] = "自动 Z 范围调整已启用", '
    '["Status.AutoZFitOff"] = "自动 Z 范围调整已关闭", ["Status.DepthBiasApplied"] = "已更新 {0} 个对象的深度偏移。", '
    '["Status.DepthBiasNoShape"] = "请先选择一个或多个 Shape。", ["Dialog.OpenTitle"] = "打开图形"',
    "Chinese depth status localization",
)

# WinForms UI.
replace_exact(
    "src/CadWinForms/MainForm.cs",
    "    private ToolStripButton? _redoButton;\n",
    "    private ToolStripButton? _redoButton;\n    private bool _autoZFitEnabled = true;\n",
    "WinForms Auto Z-fit state",
)

replace_exact(
    "src/CadWinForms/MainForm.cs",
    "        view.DropDownItems.Add(display);\n\n        var standard =",
    "        view.DropDownItems.Add(display);\n"
    "        view.DropDownItems.Add(BuildDepthMenu());\n\n"
    "        var standard =",
    "WinForms depth menu placement",
)

replace_exact(
    "src/CadWinForms/MainForm.cs",
    "        _session.Engine.SetAntialiasing(true);\n"
    "        _session.Engine.SetSelectionTolerance(4);\n",
    "        _session.Engine.SetAntialiasing(true);\n"
    "        _session.Engine.SetAutoZFitMode(true, 1.0);\n"
    "        _session.Engine.SetSelectionTolerance(4);\n",
    "WinForms Auto Z-fit initialization",
)

replace_exact(
    "src/CadWinForms/MainForm.cs",
    "    private ToolStripMenuItem BuildMaterialMenu()\n",
    "    private ToolStripMenuItem BuildDepthMenu()\n"
    "    {\n"
    "        var menu = new ToolStripMenuItem(CadLocalization.Text(\"Menu.DepthHandling\"));\n"
    "        menu.DropDownItems.Add(CheckMenuItem(\n"
    "            CadLocalization.Text(\"Menu.AutoZFit\"),\n"
    "            _autoZFitEnabled,\n"
    "            (_, item) => ExecuteSafe(() =>\n"
    "            {\n"
    "                _autoZFitEnabled = item.Checked;\n"
    "                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);\n"
    "                var message = CadLocalization.Text(\n"
    "                    _autoZFitEnabled ? \"Status.AutoZFitOn\" : \"Status.AutoZFitOff\");\n"
    "                _commandStatus.Text = message;\n"
    "                Log(message);\n"
    "            })));\n"
    "        menu.DropDownItems.Add(MenuItem(\n"
    "            CadLocalization.Text(\"Menu.AutoZFitNow\"),\n"
    "            (_, _) => ExecuteSafe(Session.Engine.AutoZFit)));\n"
    "        menu.DropDownItems.Add(new ToolStripSeparator());\n"
    "        menu.DropDownItems.Add(MenuItem(\n"
    "            CadLocalization.Text(\"Menu.DepthForward\"),\n"
    "            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Forward)));\n"
    "        menu.DropDownItems.Add(MenuItem(\n"
    "            CadLocalization.Text(\"Menu.DepthBackward\"),\n"
    "            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Backward)));\n"
    "        menu.DropDownItems.Add(MenuItem(\n"
    "            CadLocalization.Text(\"Menu.DepthReset\"),\n"
    "            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Default)));\n"
    "        return menu;\n"
    "    }\n\n"
    "    private void ApplyDepthBias(CadDepthBiasPreset preset)\n"
    "    {\n"
    "        ExecuteSafe(() =>\n"
    "        {\n"
    "            var count = Session.ApplyDepthBiasToSelection(preset);\n"
    "            var message = count == 0\n"
    "                ? CadLocalization.Text(\"Status.DepthBiasNoShape\")\n"
    "                : CadLocalization.Text(\"Status.DepthBiasApplied\", count);\n"
    "            _commandStatus.Text = message;\n"
    "            Log(message);\n"
    "        });\n"
    "    }\n\n"
    "    private ToolStripMenuItem BuildMaterialMenu()\n",
    "WinForms depth menu implementation",
)

# WPF UI.
replace_exact(
    "src/CadWpf/MainWindow.xaml.cs",
    "    private Controls.Button? _redoButton;\n",
    "    private Controls.Button? _redoButton;\n    private bool _autoZFitEnabled = true;\n",
    "WPF Auto Z-fit state",
)

replace_exact(
    "src/CadWpf/MainWindow.xaml.cs",
    "        view.Items.Add(display);\n\n        var standard =",
    "        view.Items.Add(display);\n"
    "        view.Items.Add(BuildDepthMenu());\n\n"
    "        var standard =",
    "WPF depth menu placement",
)

replace_exact(
    "src/CadWpf/MainWindow.xaml.cs",
    "            _session.Engine.SetAntialiasing(true);\n"
    "            _session.Engine.SetSelectionTolerance(4);\n",
    "            _session.Engine.SetAntialiasing(true);\n"
    "            _session.Engine.SetAutoZFitMode(true, 1.0);\n"
    "            _session.Engine.SetSelectionTolerance(4);\n",
    "WPF Auto Z-fit initialization",
)

replace_exact(
    "src/CadWpf/MainWindow.xaml.cs",
    "    private Controls.MenuItem BuildMaterialMenu()\n",
    "    private Controls.MenuItem BuildDepthMenu()\n"
    "    {\n"
    "        var menu = Menu(MenuHeader(\"Menu.DepthHandling\"));\n"
    "        menu.Items.Add(CheckMenuItem(\n"
    "            CadLocalization.Text(\"Menu.AutoZFit\"),\n"
    "            _autoZFitEnabled,\n"
    "            item => ExecuteSafe(() =>\n"
    "            {\n"
    "                _autoZFitEnabled = item.IsChecked;\n"
    "                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);\n"
    "                var message = CadLocalization.Text(\n"
    "                    _autoZFitEnabled ? \"Status.AutoZFitOn\" : \"Status.AutoZFitOff\");\n"
    "                CommandStatus.Text = message;\n"
    "                Log(message);\n"
    "            })));\n"
    "        menu.Items.Add(MenuItem(\n"
    "            CadLocalization.Text(\"Menu.AutoZFitNow\"),\n"
    "            (_, _) => ExecuteSafe(Session.Engine.AutoZFit)));\n"
    "        menu.Items.Add(new Controls.Separator());\n"
    "        menu.Items.Add(MenuItem(\n"
    "            CadLocalization.Text(\"Menu.DepthForward\"),\n"
    "            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Forward)));\n"
    "        menu.Items.Add(MenuItem(\n"
    "            CadLocalization.Text(\"Menu.DepthBackward\"),\n"
    "            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Backward)));\n"
    "        menu.Items.Add(MenuItem(\n"
    "            CadLocalization.Text(\"Menu.DepthReset\"),\n"
    "            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Default)));\n"
    "        return menu;\n"
    "    }\n\n"
    "    private void ApplyDepthBias(CadDepthBiasPreset preset)\n"
    "    {\n"
    "        ExecuteSafe(() =>\n"
    "        {\n"
    "            var count = Session.ApplyDepthBiasToSelection(preset);\n"
    "            var message = count == 0\n"
    "                ? CadLocalization.Text(\"Status.DepthBiasNoShape\")\n"
    "                : CadLocalization.Text(\"Status.DepthBiasApplied\", count);\n"
    "            CommandStatus.Text = message;\n"
    "            Log(message);\n"
    "        });\n"
    "    }\n\n"
    "    private Controls.MenuItem BuildMaterialMenu()\n",
    "WPF depth menu implementation",
)

# Demo documentation.
replace_exact(
    "README.md",
    "## Documentation\n",
    "## Coplanar display controls\n\n"
    "Both applications expose **View > Depth and Coplanar Display**. Automatic Z Range improves the camera near/far range and depth-buffer precision. It does not decide which of two exactly coplanar objects should win. For deliberate overlays, select the overlay object and use **Bring Selected Forward**, **Push Selected Back**, or **Reset Selected Depth Bias**. The commands apply one per-object polygon offset and preserve separate object identity, selection and properties. Duplicate production geometry should still be removed or hidden.\n\n"
    "## Documentation\n",
    "English Demo depth documentation",
)

replace_exact(
    "README.zh-CN.md",
    "## 文档\n",
    "## 共面显示控制\n\n"
    "WinForms 和 WPF 均在 **视图 > 深度与共面显示** 中提供控制。自动 Z 范围用于优化相机近远裁剪范围和深度缓冲精度，但不能决定两个完全共面对象谁显示在前。对于有意叠加的预览面、参考面或覆盖对象，应先选择需要调整的对象，再使用 **将选中对象前移**、**将选中对象后移** 或 **恢复选中对象深度偏移**。这些命令只修改对象级 Polygon Offset，不合并对象，也不影响独立选择和属性。正式模型中的重复几何仍应删除或隐藏。\n\n"
    "## 文档\n",
    "Chinese Demo depth documentation",
)
