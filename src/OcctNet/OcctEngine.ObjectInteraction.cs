namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetSelectable(IOcctObject value, bool selectable)
    {
        ArgumentNullException.ThrowIfNull(value);
        CheckInitialized(() => NativeMethods.occt_set_object_selectable(_handle, value.Id, selectable ? 1 : 0));
    }

    public bool IsSelectable(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureNotDisposed();
        return NativeMethods.occt_get_object_selectable(_handle, value.Id) != 0;
    }

    public void SetSelectable(IEnumerable<IOcctObject> values, bool selectable)
    {
        ArgumentNullException.ThrowIfNull(values);
        EnsureInitialized();
        var ids = values.Select(value => value?.Id ?? throw new ArgumentException("Object collection contains null.", nameof(values)))
            .Where(id => id > 0)
            .Distinct()
            .ToArray();
        if (ids.Length == 0) return;
        Check(NativeMethods.occt_set_objects_selectable(_handle, ids, ids.Length, selectable ? 1 : 0));
    }

    public void SetViewCubeLanguage(OcctViewCubeLanguage language) =>
        CheckInitialized(() => NativeMethods.occt_set_view_cube_language(_handle, (int)language));
}
