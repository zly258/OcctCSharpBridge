namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetSelectable(IOcctObject value, bool selectable)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_set_object_selectable(_handle, value.Id, selectable ? 1 : 0));
    }

    public bool IsSelectable(IOcctObject value)
    {
        EnsureObject(value);
        return NativeMethods.occt_get_object_selectable(_handle, value.Id) != 0;
    }

    public void SetSelectable(IEnumerable<IOcctObject> values, bool selectable)
    {
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;
        CheckInitialized(() => NativeMethods.occt_set_objects_selectable(_handle, ids, ids.Length, selectable ? 1 : 0));
    }

    public void Highlight(IOcctObject value)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_highlight_object(_handle, value.Id));
    }

    public void Unhighlight(IOcctObject value)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_unhighlight_object(_handle, value.Id));
    }
}
