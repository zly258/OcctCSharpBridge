from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def read(path: str) -> str:
    return (ROOT / path).read_bytes().decode("utf-8-sig").replace("\r\n", "\n")


def write(path: str, text: str) -> None:
    normalized = text.replace("\r\n", "\n").replace("\n", "\r\n")
    (ROOT / path).write_bytes(BOM + normalized.encode("utf-8"))


def replace_once(text: str, old: str, new: str, label: str) -> str:
    if old not in text:
        raise RuntimeError(f"Expected block not found: {label}")
    return text.replace(old, new, 1)


session_path = "src/CadCommon/CadSession.cs"
session = read(session_path)
session = replace_once(
    session,
    "public sealed class CadSession\n",
    "public sealed partial class CadSession\n",
    "partial CadSession declaration")
session = replace_once(
    session,
    "    public CadCommandResult Execute(CadCommandId commandId, IReadOnlyDictionary<string, string>? rawValues = null)\n    {\n        var storedValues = rawValues is null\n",
    "    public CadCommandResult Execute(CadCommandId commandId, IReadOnlyDictionary<string, string>? rawValues = null)\n    {\n        EnsureCommandAvailable(commandId);\n\n        var storedValues = rawValues is null\n",
    "execution safeguard")
session = replace_once(
    session,
    "        var selectedObjectIds = Engine.SelectedObjects.Select(item => item.Id).Distinct().ToList();\n        if (selectedObjectIds.Count == 0 && ActiveObject is { } active && Engine.Exists(active))\n        {\n            selectedObjectIds.Add(active.Id);\n        }\n",
    "        var selectedObjectIds = Engine.SelectedObjects.Select(item => item.Id).Distinct().ToList();\n",
    "history selection fallback")
session = replace_once(
    session,
    "        var selected = Engine.SelectedObjects.ToList();\n        if (selected.Count == 0 && ActiveObject is { } active) selected.Add(active);\n\n        var targets = selected\n",
    "        var selected = Engine.SelectedObjects.ToList();\n\n        var targets = selected\n",
    "delete selection fallback")
session = replace_once(
    session,
    "    private List<OcctShape> SelectedShapes()\n    {\n        var shapes = Engine.SelectedObjects.Where(item => item.Kind == OcctObjectKind.Shape).Select(item => new OcctShape(item.Id)).DistinctBy(item => item.Id).ToList();\n        if (shapes.Count == 0 && ActiveObject is { Kind: OcctObjectKind.Shape } active && Engine.Exists(active)) shapes.Add(new OcctShape(active.Id));\n        return shapes;\n    }\n",
    "    private List<OcctShape> SelectedShapes()\n    {\n        return Engine.SelectedObjects\n            .Where(item => item.Kind == OcctObjectKind.Shape)\n            .Select(item => new OcctShape(item.Id))\n            .DistinctBy(item => item.Id)\n            .ToList();\n    }\n",
    "explicit selected shapes")
write(session_path, session)

winforms_path = "src/CadWinForms/MainForm.cs"
winforms = read(winforms_path)
winforms = replace_once(
    winforms,
    "    private void RunCommand(CadCommandId id)\n    {\n        if (_session is null) return;\n        var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));\n",
    "    private void RunCommand(CadCommandId id)\n    {\n        if (_session is null) return;\n\n        var availability = _session.GetCommandAvailability(id);\n        if (!availability.CanExecute)\n        {\n            ReportCommandPrecondition(availability.Message);\n            return;\n        }\n\n        var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));\n",
    "WinForms early precondition")
winforms = replace_once(
    winforms,
    "    private void RunCommand(CadCommandId id)\n",
    "    private void ReportCommandPrecondition(string message)\n    {\n        _commandStatus.Text = message;\n        Log(message);\n        System.Media.SystemSounds.Asterisk.Play();\n        _viewport.Focus();\n    }\n\n    private void RunCommand(CadCommandId id)\n",
    "WinForms precondition reporter")
write(winforms_path, winforms)

wpf_path = "src/CadWpf/MainWindow.xaml.cs"
wpf = read(wpf_path)
wpf = replace_once(
    wpf,
    "    private void RunCommand(CadCommandId id)\n    {\n        if (_session is null) return;\n        var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));\n",
    "    private void RunCommand(CadCommandId id)\n    {\n        if (_session is null) return;\n\n        var availability = _session.GetCommandAvailability(id);\n        if (!availability.CanExecute)\n        {\n            ReportCommandPrecondition(availability.Message);\n            return;\n        }\n\n        var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));\n",
    "WPF early precondition")
wpf = replace_once(
    wpf,
    "    private void RunCommand(CadCommandId id)\n",
    "    private void ReportCommandPrecondition(string message)\n    {\n        CommandStatus.Text = message;\n        Log(message);\n        System.Media.SystemSounds.Asterisk.Play();\n        Viewport.FocusViewport();\n    }\n\n    private void RunCommand(CadCommandId id)\n",
    "WPF precondition reporter")
write(wpf_path, wpf)

build_path = "build.ps1"
build = read(build_path)
build = replace_once(
    build,
    '    Package = "tests\\check-demo-package.ps1"\n',
    '    DemoPreconditions = "tests\\check-demo-command-preconditions.ps1"\n    Package = "tests\\check-demo-package.ps1"\n',
    "demo precondition build check")
write(build_path, build)

# The workflow is one-shot; the Python migration file removes itself from the resulting commit.
(ROOT / ".github/apply_demo_command_preconditions.py").unlink()
