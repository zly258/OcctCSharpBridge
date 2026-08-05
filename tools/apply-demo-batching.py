from pathlib import Path


def read(path: str) -> str:
    return Path(path).read_text(encoding="utf-8-sig")


def write(path: str, text: str) -> None:
    Path(path).write_text(text, encoding="utf-8", newline="\n")


def replace_once(text: str, old: str, new: str, label: str) -> str:
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{label}: expected exactly one match, found {count}")
    return text.replace(old, new, 1)


# Batch every Demo* command in the shared session used by WinForms and WPF.
path = "src/CadCommon/CadSession.cs"
text = read(path)
text = replace_once(
    text,
    "        var values = new CadValues(storedValues);\n        var result = commandId switch\n        {\n",
    "        var values = new CadValues(storedValues);\n"
    "        var displayBatch = IsDemoCommand(commandId) ? Engine.BeginDisplayBatch() : null;\n"
    "        CadCommandResult result;\n"
    "        try\n"
    "        {\n"
    "            result = commandId switch\n"
    "            {\n",
    "open demo display batch")
text = replace_once(
    text,
    "        };\n\n        var changed = result.CreatedObjects.Count > 0 || commandId == CadCommandId.Delete;\n",
    "            };\n"
    "        }\n"
    "        finally\n"
    "        {\n"
    "            displayBatch?.Dispose();\n"
    "        }\n\n"
    "        var changed = result.CreatedObjects.Count > 0 || commandId == CadCommandId.Delete;\n",
    "close demo display batch")
text = replace_once(
    text,
    "    private static string Local(string english, string chinese) =>\n",
    "    private static bool IsDemoCommand(CadCommandId commandId) =>\n"
    "        commandId is CadCommandId.DemoPrimitives\n"
    "            or CadCommandId.DemoBracket\n"
    "            or CadCommandId.DemoFlange\n"
    "            or CadCommandId.DemoPipe\n"
    "            or CadCommandId.DemoTee\n"
    "            or CadCommandId.DemoReducer\n"
    "            or CadCommandId.DemoLoft\n"
    "            or CadCommandId.DemoBoolean\n"
    "            or CadCommandId.DemoAnnotations;\n\n"
    "    private static string Local(string english, string chinese) =>\n",
    "add demo command classifier")
write(path, text)

# The API Center can run several sample commands; keep their nested batches under one final redraw.
path = "src/CadCommon/ApiDemo.cs"
text = read(path)
old = '''        foreach (var command in commands)\n        {\n            token.ThrowIfCancellationRequested();\n            progress.Report($"Running {CadLocalization.CommandText(command)}...");\n            session.Execute(command);\n        }\n        session.Engine.FitAll();\n'''
new = '''        using (session.Engine.BeginDisplayBatch())\n        {\n            foreach (var command in commands)\n            {\n                token.ThrowIfCancellationRequested();\n                progress.Report($"Running {CadLocalization.CommandText(command)}...");\n                session.Execute(command);\n            }\n            session.Engine.FitAll();\n        }\n'''
text = replace_once(text, old, new, "batch API Center sample scenario")
write(path, text)

print("Demo batching patch applied.")
