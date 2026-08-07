namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetSelection(
        IEnumerable<IOcctObject> values,
        OcctSelectionOperation operation = OcctSelectionOperation.Replace)
    {
        ArgumentNullException.ThrowIfNull(values);
        EnsureInitialized();
        if (operation == OcctSelectionOperation.Clear)
        {
            Check(NativeMethods.occt_set_selected_objects_ex(_handle, null, 0, (int)operation));
            return;
        }

        var ids = values.Select(value => value?.Id ?? throw new ArgumentException("Selection contains null.", nameof(values)))
            .Where(id => id > 0)
            .Distinct()
            .ToArray();
        Check(NativeMethods.occt_set_selected_objects_ex(_handle, ids, ids.Length, (int)operation));
    }

    public IReadOnlyList<IOcctObject> GetSelectedObjects() => SelectedObjects.Cast<IOcctObject>().ToArray();
}
