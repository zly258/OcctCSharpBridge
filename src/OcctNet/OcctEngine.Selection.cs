namespace OcctNet;

public sealed partial class OcctEngine
{
    public void MoveTo(int x, int y) =>
        CheckInitialized(() => NativeMethods.occt_move_to(_handle, x, y));

    public void Select(int x, int y, bool appendSelection = false) =>
        CheckInitialized(() => NativeMethods.occt_select(_handle, x, y, appendSelection ? 1 : 0));

    public void SelectRectangle(
        int x1,
        int y1,
        int x2,
        int y2,
        bool appendSelection = false,
        bool allowOverlap = false) =>
        CheckInitialized(() => NativeMethods.occt_select_rectangle_ex(
            _handle,
            x1,
            y1,
            x2,
            y2,
            appendSelection ? 1 : 0,
            allowOverlap ? 1 : 0));

    public void SelectObject(OcctObject value, bool appendSelection = false)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_select_object(_handle, value.Id, appendSelection ? 1 : 0));
    }

    public void SetSelectionMode(OcctSelectionMode mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        CheckInitialized(() => NativeMethods.occt_set_selection_mode(_handle, (int)mode));
    }

    public IReadOnlyList<OcctObject> SelectedObjects
    {
        get
        {
            EnsureInitialized();
            var count = NativeMethods.occt_selected_count(_handle);
            var result = new List<OcctObject>(Math.Max(count, 0));
            for (var index = 0; index < count; index++)
            {
                var id = NativeMethods.occt_selected_at(_handle, index);
                if (id > 0) result.Add(new OcctObject(id, GetObjectKind(id), _ownerId));
            }
            return result;
        }
    }

    public OcctShape? FirstSelected
    {
        get
        {
            EnsureInitialized();
            var id = NativeMethods.occt_selected_at(_handle, 0);
            return id > 0 && GetObjectKind(id) == OcctObjectKind.Shape
                ? new OcctShape(id, _ownerId)
                : null;
        }
    }

    public OcctObject? FirstSelectedObject
    {
        get
        {
            EnsureInitialized();
            var id = NativeMethods.occt_selected_at(_handle, 0);
            return id > 0 ? new OcctObject(id, GetObjectKind(id), _ownerId) : null;
        }
    }

    public void ClearSelection() =>
        CheckInitialized(() => NativeMethods.occt_clear_selection(_handle));

    public OcctShape CopySelectedSubshape() => CopySelectedSubshape(0);

    public OcctShape CopySelectedSubshape(int index)
    {
        EnsureInitialized();
        OcctGuard.PositiveIndex(index, nameof(index));
        return CheckShape(NativeMethods.occt_copy_selected_subshape_at(_handle, index));
    }
}
