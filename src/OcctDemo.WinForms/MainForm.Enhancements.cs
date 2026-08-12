using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    private bool _enhancementsWired;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_enhancementsWired) return;
        _enhancementsWired = true;

        _selectionCombo.SelectedIndexChanged += (_, _) =>
        {
            if (_selectionCombo.SelectedIndex >= 0)
            {
                SetSelectionMode((OcctSelectionMode)_selectionCombo.SelectedIndex);
            }
        };

        _viewport.ObjectSelectionChanged += (_, args) =>
        {
            if (_session is null || args.SelectedObjects.Count <= 1) return;
            _session.ActiveObject = null;
            _propertyGrid.Rows.Clear();
            _propertyGrid.Rows.Add(
                Local("Selection", "选择"),
                Local($"{args.SelectedObjects.Count} objects selected", $"已选择 {args.SelectedObjects.Count} 个对象"));
        };
    }
}
