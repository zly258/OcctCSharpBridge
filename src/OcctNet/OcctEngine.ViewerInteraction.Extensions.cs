namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetSelectionModeActive(
        IOcctObject value,
        OcctSelectionMode mode,
        bool active,
        OcctSelectionModeConcurrency concurrency = OcctSelectionModeConcurrency.Multiple,
        bool force = false)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        if (!Enum.IsDefined(concurrency)) throw new ArgumentOutOfRangeException(nameof(concurrency));
        CheckInitialized(() => ViewerInteractionExtensionsNativeMethods.occt_set_object_selection_mode_active(
            _handle,
            value.Id,
            (int)mode,
            active ? 1 : 0,
            (int)concurrency,
            force ? 1 : 0));
    }

    public void SetSelectionSensitivity(
        IOcctObject value,
        OcctSelectionMode mode,
        int sensitivity)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        if (sensitivity <= 0)
            throw new ArgumentOutOfRangeException(nameof(sensitivity), "Selection sensitivity must be greater than zero.");
        CheckInitialized(() => ViewerInteractionExtensionsNativeMethods.occt_set_object_selection_sensitivity(
            _handle,
            value.Id,
            (int)mode,
            sensitivity));
    }

    public void SetDisplayPriority(IOcctObject value, int priority)
    {
        EnsureObject(value);
        ValidateDisplayPriority(priority);
        CheckInitialized(() => ViewerInteractionExtensionsNativeMethods.occt_set_object_display_priority(
            _handle,
            value.Id,
            priority));
    }

    public void SetDisplayPriority(IEnumerable<IOcctObject> values, int priority)
    {
        ArgumentNullException.ThrowIfNull(values);
        ValidateDisplayPriority(priority);
        EnsureInitialized();

        var items = values.ToArray();
        var ids = new long[items.Length];
        for (var index = 0; index < items.Length; index++)
        {
            ArgumentNullException.ThrowIfNull(items[index]);
            EnsureObject(items[index]);
            ids[index] = items[index].Id;
        }
        if (ids.Length == 0) return;

        Check(ViewerInteractionExtensionsNativeMethods.occt_set_objects_display_priority(
            _handle,
            ids,
            ids.Length,
            priority));
    }

    public int GetDisplayPriority(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        Check(ViewerInteractionExtensionsNativeMethods.occt_get_object_display_priority(
            _handle,
            value.Id,
            out var priority));
        ValidateDisplayPriority(priority);
        return priority;
    }

    public void SetTransformPersistence(
        IOcctObject value,
        OcctTransformPersistenceMode mode,
        OcctPoint3d anchor)
    {
        EnsureObject(value);
        if (mode is not OcctTransformPersistenceMode.Zoom
            and not OcctTransformPersistenceMode.Rotate
            and not OcctTransformPersistenceMode.ZoomRotate)
        {
            throw new ArgumentOutOfRangeException(nameof(mode), "Mode must use a 3D anchor.");
        }
        OcctGuard.Finite(anchor, nameof(anchor));
        CheckInitialized(() => ViewerInteractionExtensionsNativeMethods.occt_set_object_transform_persistence_3d(
            _handle,
            value.Id,
            (int)mode,
            anchor));
    }

    public void SetTransformPersistence(
        IOcctObject value,
        OcctTransformPersistenceMode mode,
        OcctCornerPosition position,
        int offsetX = 0,
        int offsetY = 0)
    {
        EnsureObject(value);
        if (mode is not OcctTransformPersistenceMode.Screen2d
            and not OcctTransformPersistenceMode.Triedron)
        {
            throw new ArgumentOutOfRangeException(nameof(mode), "Mode must use a screen anchor.");
        }
        if (!Enum.IsDefined(position)) throw new ArgumentOutOfRangeException(nameof(position));
        if (offsetX < 0) throw new ArgumentOutOfRangeException(nameof(offsetX));
        if (offsetY < 0) throw new ArgumentOutOfRangeException(nameof(offsetY));

        CheckInitialized(() => ViewerInteractionExtensionsNativeMethods.occt_set_object_transform_persistence_2d(
            _handle,
            value.Id,
            (int)mode,
            (int)position,
            offsetX,
            offsetY));
    }

    public void ClearTransformPersistence(IOcctObject value)
    {
        EnsureObject(value);
        CheckInitialized(() => ViewerInteractionExtensionsNativeMethods.occt_clear_object_transform_persistence(
            _handle,
            value.Id));
    }

    public OcctTransformPersistenceState GetTransformPersistence(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        Check(ViewerInteractionExtensionsNativeMethods.occt_get_object_transform_persistence(
            _handle,
            value.Id,
            out var native));

        if (!Enum.IsDefined(typeof(OcctTransformPersistenceMode), native.Mode))
            throw new InvalidOperationException($"Native transform persistence mode {native.Mode} is not supported.");
        var mode = (OcctTransformPersistenceMode)native.Mode;
        var position = OcctCornerPosition.LeftLower;
        if (mode is OcctTransformPersistenceMode.Screen2d or OcctTransformPersistenceMode.Triedron)
        {
            if (!Enum.IsDefined(typeof(OcctCornerPosition), native.Position))
                throw new InvalidOperationException($"Native transform persistence corner {native.Position} is not supported.");
            position = (OcctCornerPosition)native.Position;
        }

        return new(mode, native.Anchor, position, native.OffsetX, native.OffsetY);
    }

    public int ViewClipPlaneLimit
    {
        get
        {
            EnsureInitialized();
            Check(ViewerInteractionExtensionsNativeMethods.occt_get_view_clip_plane_limit(
                _handle,
                out var limit));
            if (limit < 0) throw new InvalidOperationException("Native view clip plane limit is invalid.");
            return limit;
        }
    }

    public void SetViewClipPlanes(IReadOnlyList<OcctViewClipPlane> planes)
    {
        ArgumentNullException.ThrowIfNull(planes);
        EnsureInitialized();
        if (planes.Count > ViewClipPlaneLimit)
            throw new ArgumentException("Clip plane count exceeds the current view limit.", nameof(planes));

        var native = new NativeOcctViewClipPlane[planes.Count];
        for (var index = 0; index < planes.Count; index++)
        {
            var plane = planes[index] ?? throw new ArgumentException("Clip plane entries must not be null.", nameof(planes));
            OcctGuard.Finite(plane.Point, nameof(planes));
            OcctGuard.NonZero(plane.Normal, nameof(planes));
            native[index] = new NativeOcctViewClipPlane
            {
                Point = plane.Point,
                Normal = plane.Normal,
                Enabled = plane.Enabled ? 1 : 0,
                Capping = plane.Capping ? 1 : 0,
                CappingR = plane.CappingColor.R / 255.0,
                CappingG = plane.CappingColor.G / 255.0,
                CappingB = plane.CappingColor.B / 255.0
            };
        }

        Check(ViewerInteractionExtensionsNativeMethods.occt_set_view_clip_planes(
            _handle,
            native,
            native.Length));
    }

    public void ClearViewClipPlanes() => SetViewClipPlanes(Array.Empty<OcctViewClipPlane>());

    public void UpdatePoints(IReadOnlyList<OcctPointStateUpdate> updates)
    {
        ArgumentNullException.ThrowIfNull(updates);
        EnsureInitialized();
        if (updates.Count == 0) return;

        var native = new NativeOcctPointStateUpdate[updates.Count];
        for (var index = 0; index < updates.Count; index++)
        {
            var update = updates[index];
            EnsurePoint(update.Point);
            OcctGuard.Finite(update.Position, nameof(updates));
            native[index] = new NativeOcctPointStateUpdate
            {
                PointId = update.Point.Id,
                Position = update.Position,
                Visible = update.Visible ? 1 : 0
            };
        }

        Check(ViewerInteractionExtensionsNativeMethods.occt_update_points(
            _handle,
            native,
            native.Length));
    }

    private static void ValidateDisplayPriority(int priority)
    {
        if (priority < 0 || priority > 10)
            throw new ArgumentOutOfRangeException(nameof(priority), "Display priority must be between 0 and 10.");
    }
}
