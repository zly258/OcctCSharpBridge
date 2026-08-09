using OcctNet;

namespace CadCommon;

public sealed partial class CadSession
{
    public void Undo()
    {
        if (!CanUndo)
        {
            StatusChanged?.Invoke(this, CadLocalization.Text("History.NothingToUndo"));
            return;
        }
        var description = DescribeHistoryEntry(_history[_historyPosition - 1]);
        _historyPosition--;
        RebuildFromHistory();
        StatusChanged?.Invoke(this, CadLocalization.Text("History.Undone", description));
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Redo()
    {
        if (!CanRedo)
        {
            StatusChanged?.Invoke(this, CadLocalization.Text("History.NothingToRedo"));
            return;
        }
        var description = DescribeHistoryEntry(_history[_historyPosition]);
        _historyPosition++;
        RebuildFromHistory();
        StatusChanged?.Invoke(this, CadLocalization.Text("History.Redone", description));
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RebuildFromHistory()
    {
        _restoringHistory = true;
        _suppressNotifications = true;
        try
        {
            Engine.Clear();
            ActiveObject = null;
            _nameSequence = 1;

            if (!string.IsNullOrWhiteSpace(_historyBaselineFile))
            {
                ImportCore(_historyBaselineFile);
            }

            for (var index = 0; index < _historyPosition; index++)
            {
                var entry = _history[index];
                if (entry.IsImport)
                {
                    ImportCore(entry.ImportFilePath!);
                    continue;
                }

                if (entry.CommandId is not { } commandId) continue;
                Engine.ClearSelection();
                var append = false;
                foreach (var objectId in entry.SelectedObjectIds)
                {
                    var value = Engine.GetObject(objectId);
                    if (!Engine.Exists(value)) continue;
                    Engine.SelectObject(value, append);
                    append = true;
                }
                Execute(commandId, entry.Values);
            }
            Engine.ClearSelection();
            if (Engine.ShapeCount > 0) Engine.FitAll();
            IsModified = _historyPosition > 0;
        }
        finally
        {
            _suppressNotifications = false;
            _restoringHistory = false;
        }
        ModelChanged?.Invoke(this, EventArgs.Empty);
    }

    private static string DescribeHistoryEntry(CadHistoryEntry entry)
    {
        if (entry.CommandId is { } commandId)
        {
            return CadLocalization.CommandText(commandId);
        }

        if (entry.IsImport)
        {
            var fileName = Path.GetFileName(entry.ImportFilePath);
            return CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
                ? $"导入 {fileName}"
                : $"Import {fileName}";
        }

        return entry.Description;
    }

    private void TruncateRedoHistory()
    {
        if (_historyPosition < _history.Count)
        {
            _history.RemoveRange(_historyPosition, _history.Count - _historyPosition);
        }
    }

    private void ClearHistory()
    {
        _history.Clear();
        _historyPosition = 0;
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool IsUndoableCommand(CadCommandId commandId) => commandId is not
        (CadCommandId.AnalyzeBounds or CadCommandId.AnalyzeMass or CadCommandId.AnalyzeTopology or
         CadCommandId.AnalyzeDistance or CadCommandId.ValidateShape or
         CadCommandId.LengthDimension or CadCommandId.AngleDimension or
         CadCommandId.RadiusDimension or CadCommandId.DiameterDimension);

}
