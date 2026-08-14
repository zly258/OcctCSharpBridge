using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetObjectSelectable(IOcctObject value, bool selectable)
    {
        EnsureObject(value);
        var options = ObjectUpdateOptions(NativeViewerObjectUpdateMask.Selectable);
        options.Selectable = selectable ? 1 : 0;
        UpdateObject(value.Id, options);
    }

    public void SetObjectsSelectable(IEnumerable<IOcctObject> values, bool selectable)
    {
        ArgumentNullException.ThrowIfNull(values);
        EnsureInitialized();
        var ids = GetObjectIds(values, nameof(values));
        var options = ObjectUpdateOptions(NativeViewerObjectUpdateMask.Selectable);
        options.Selectable = selectable ? 1 : 0;
        foreach (var id in ids) UpdateObject(id, options);
    }

    public bool IsObjectSelectable(IOcctObject value) => GetObjectState(value).Selectable != 0;

    public bool IsObjectVisible(IOcctObject value) => GetObjectState(value).Visible != 0;

    public bool IsObjectSelected(IOcctObject value) => GetObjectState(value).Selected != 0;

    public bool IsObjectHighlighted(IOcctObject value) => GetObjectState(value).Highlighted != 0;

    public void ShowAll()
    {
        EnsureInitialized();
        CheckObjectStatus(ObjectNativeMethods.occt_engine_objects_visibility_all_set(_handle, 1));
    }

    public void HideAll()
    {
        EnsureInitialized();
        CheckObjectStatus(ObjectNativeMethods.occt_engine_objects_visibility_all_set(_handle, 0));
    }

    public void Redisplay(IOcctObject value) =>
        ApplyPresentationAction(value, NativeViewerObjectPresentationAction.Redisplay);

    public void Highlight(IOcctObject value) =>
        ApplyPresentationAction(value, NativeViewerObjectPresentationAction.Highlight);

    public void Unhighlight(IOcctObject value) =>
        ApplyPresentationAction(value, NativeViewerObjectPresentationAction.Unhighlight);

    private NativeViewerObjectState GetObjectState(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckObjectStatus(ObjectNativeMethods.occt_engine_object_state_get(_handle, value.Id, out var state));
        if (state.ApiVersion != 1 || state.StructSize < (uint)Marshal.SizeOf<NativeViewerObjectState>())
            throw new OcctException("Native object state ABI is incompatible with this SDK.");
        return state;
    }

    private void ApplyPresentationAction(IOcctObject value, NativeViewerObjectPresentationAction action)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckObjectStatus(ObjectNativeMethods.occt_engine_object_presentation_action(_handle, value.Id, action));
    }
}
