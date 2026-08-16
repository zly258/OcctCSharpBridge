using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void MoveTo(int x, int y)
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_move_to(_handle, x, y));
        UpdateDetectedHit(x, y);
    }

    public void Select(int x, int y, bool appendSelection = false)
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_point_select(
            _handle,
            x,
            y,
            appendSelection ? 1 : 0));
    }

    public void SelectRectangle(
        int x1,
        int y1,
        int x2,
        int y2,
        bool appendSelection = false,
        bool allowOverlap = false)
    {
        var options = new NativeViewerRectangleSelectionOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerRectangleSelectionOptions>(),
            ApiVersion = 1,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Append = appendSelection ? 1 : 0,
            AllowOverlap = allowOverlap ? 1 : 0
        };
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_rectangle_select(
            _handle,
            in options));
    }

    public void SelectObject(IOcctObject value, bool appendSelection = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureObject(value);
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_object_select(
            _handle,
            value.Id,
            appendSelection ? 1 : 0));
    }

    public void SelectAllVisible()
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_all_visible(_handle));
    }

    public void InvertSelection()
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_invert(_handle));
    }

    public void HideSelected()
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_hide_selected(_handle));
    }

    public void SetAutomaticHighlight(bool enabled)
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_automatic_highlight_set(
            _handle,
            enabled ? 1 : 0));
    }

    public void SetSelectionMode(OcctSelectionMode mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        UpdateSelectionSettings(new NativeViewerSelectionSettingsOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerSelectionSettingsOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerSelectionSettingsUpdateMask.Mode,
            SelectionMode = (int)mode
        });
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
                if (seen.Add(hit.Owner.Id)) result.Add(hit.Owner);
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

    public void ClearSelection()
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_clear(_handle));
    }

    public OcctShape CopySelectedSubshape() => CopySelectedSubshape(0);

    public OcctShape CopySelectedSubshape(int index)
    {
        EnsureInitialized();
        OcctGuard.PositiveIndex(index, nameof(index));
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_subshape_copy(
            _handle,
            index,
            out var shapeId));
        if (shapeId <= 0) throw new OcctException("Native selection copy returned an invalid shape ID.");
        return new OcctShape(shapeId, _ownerId);
    }

    private void UpdateSelectionSettings(NativeViewerSelectionSettingsOptions options)
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_settings_update(_handle, in options));
    }

    private void CheckSelectionStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
