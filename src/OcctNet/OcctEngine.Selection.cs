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

    public void SelectObject(IOcctObject value, bool appendSelection = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_select_object(_handle, value.Id, appendSelection ? 1 : 0));
    }

    public void SetSelectionMode(OcctSelectionMode mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        CheckInitialized(() => NativeMethods.occt_set_selection_mode(_handle, (int)mode));
    }

    public IReadOnlyList<IOcctObject> SelectedObjects
    {
        get
        {
            EnsureInitialized();
            var hits = GetSelectedHits();
            if (hits.Count == 0) return Array.Empty<IOcctObject>();

            var result = new List<IOcctObject>(hits.Count);
            var seen = new HashSet<long>();
            foreach (var hit in hits)
            {
                if (seen.Add(hit.Owner.Id))
                    result.Add(hit.Owner);
            }
            return result;
        }
    }

    public IOcctObject? FirstSelectedObject
    {
        get
        {
            var selected = SelectedObjects;
            return selected.Count == 0 ? null : selected[0];
        }
    }

    public OcctShape? FirstSelected =>
        FirstSelectedObject is OcctShape shape ? shape : null;

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
