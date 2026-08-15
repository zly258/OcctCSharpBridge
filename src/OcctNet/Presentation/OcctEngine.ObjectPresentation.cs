using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetDisplayPriority(IOcctObject value, int priority)
    {
        EnsureObject(value);
        ValidateDisplayPriority(priority);
        EnsureInitialized();
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_object_display_priority_set(
            _handle,
            value.Id,
            priority));
    }

    public void SetDisplayPriority(IEnumerable<IOcctObject> values, int priority)
    {
        ValidateDisplayPriority(priority);
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;

        GCHandle pinned = default;
        try
        {
            pinned = GCHandle.Alloc(ids, GCHandleType.Pinned);
            EnsureInitialized();
            CheckPresentationStatus(PresentationNativeMethods.occt_engine_objects_display_priority_set(
                _handle,
                pinned.AddrOfPinnedObject(),
                ids.Length,
                priority));
        }
        finally
        {
            if (pinned.IsAllocated) pinned.Free();
        }
    }

    public int GetDisplayPriority(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_object_display_priority_get(
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

        var options = new NativeViewerTransformPersistenceOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerTransformPersistenceOptions>(),
            ApiVersion = 1,
            Mode = (int)mode,
            Anchor = anchor
        };
        EnsureInitialized();
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_object_transform_persistence_set(
            _handle,
            value.Id,
            in options));
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

        var options = new NativeViewerTransformPersistenceOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerTransformPersistenceOptions>(),
            ApiVersion = 1,
            Mode = (int)mode,
            Position = (int)position,
            OffsetX = offsetX,
            OffsetY = offsetY
        };
        EnsureInitialized();
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_object_transform_persistence_set(
            _handle,
            value.Id,
            in options));
    }

    public void ClearTransformPersistence(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_object_transform_persistence_clear(
            _handle,
            value.Id));
    }

    public OcctTransformPersistenceState GetTransformPersistence(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_object_transform_persistence_get(
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

    private static void ValidateDisplayPriority(int priority)
    {
        if (priority < 0 || priority > 10)
            throw new ArgumentOutOfRangeException(nameof(priority), "Display priority must be between 0 and 10.");
    }
}
