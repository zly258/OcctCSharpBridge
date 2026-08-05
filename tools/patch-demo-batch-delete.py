from pathlib import Path

path = Path("src/CadCommon/CadSession.cs")
text = path.read_text(encoding="utf-8-sig")

old = '''    private CadCommandResult DeleteSelected()
    {
        var selected = Engine.SelectedObjects.ToList();
        if (selected.Count == 0 && ActiveObject is { } active) selected.Add(active);
        if (selected.Count == 0) throw new InvalidOperationException(Local("Select one or more objects to erase.", "请先选择要删除的对象。"));
        foreach (var value in selected.DistinctBy(item => item.Id)) if (Engine.Exists(value)) Engine.Delete(value);
        ActiveObject = null;
        return CadCommandResult.Empty(CadLocalization.Text("Session.Deleted", selected.Count));
    }
'''

new = '''    private CadCommandResult DeleteSelected()
    {
        var selected = Engine.SelectedObjects.ToList();
        if (selected.Count == 0 && ActiveObject is { } active) selected.Add(active);

        var targets = selected
            .DistinctBy(item => item.Id)
            .Where(Engine.Exists)
            .Select(item => (IOcctObject)item)
            .ToArray();

        if (targets.Length == 0)
        {
            throw new InvalidOperationException(Local(
                "Select one or more objects to erase.",
                "请先选择要删除的对象。"));
        }

        // One managed call, one P/Invoke transition, one native validation pass and one redraw.
        Engine.Delete(targets);
        ActiveObject = null;
        return CadCommandResult.Empty(CadLocalization.Text("Session.Deleted", targets.Length));
    }
'''

if old not in text:
    if new in text:
        print("Demo batch delete is already applied.")
        raise SystemExit(0)
    raise SystemExit("DeleteSelected block was not found.")

path.write_text(text.replace(old, new, 1), encoding="utf-8", newline="\n")
print("Updated CadSession.DeleteSelected to use native batch deletion.")
