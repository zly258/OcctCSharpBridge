using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public int ShapeCount
    {
        get
        {
            GetObjectCounts(out _, out var shapeCount);
            return shapeCount;
        }
    }

    public int ObjectCount
    {
        get
        {
            GetObjectCounts(out var objectCount, out _);
            return objectCount;
        }
    }

    public bool ContainsObject(long objectId)
    {
        EnsureNotDisposed();
        if (objectId <= 0) return false;
        CheckObjectStatus(ObjectNativeMethods.occt_engine_object_exists(_handle, objectId, out var exists));
        return exists != 0;
    }

    public OcctObjectKind GetObjectKind(long objectId)
    {
        EnsureNotDisposed();
        if (objectId <= 0) throw new ArgumentOutOfRangeException(nameof(objectId));
        CheckObjectStatus(ObjectNativeMethods.occt_engine_object_kind_get(_handle, objectId, out var rawKind));
        if (!Enum.IsDefined(typeof(OcctObjectKind), rawKind))
            throw new OcctException($"Native object kind {rawKind} is not supported by this SDK.");
        return (OcctObjectKind)rawKind;
    }

    public IOcctObject GetObject(long objectId)
    {
        if (!ContainsObject(objectId)) throw new ArgumentException("Object ID does not exist.", nameof(objectId));
        return CreateObject(objectId, GetObjectKind(objectId));
    }

    public IReadOnlyList<OcctObjectDescriptor> GetObjectDescriptors()
    {
        EnsureNotDisposed();
        GetObjectCounts(out var objectCount, out _);
        if (objectCount == 0) return Array.Empty<OcctObjectDescriptor>();

        var elementSize = Marshal.SizeOf<OcctObjectDescriptorNative>();
        var buffer = Marshal.AllocHGlobal(checked(elementSize * objectCount));
        try
        {
            CheckObjectStatus(ObjectNativeMethods.occt_engine_objects_snapshot_get(
                _handle,
                buffer,
                objectCount,
                out var filledCount,
                out _));
            if (filledCount < 0 || filledCount > objectCount)
                throw new OcctException("Native object descriptor count is invalid.");

            var result = new OcctObjectDescriptor[filledCount];
            for (var index = 0; index < filledCount; index++)
            {
                var native = Marshal.PtrToStructure<OcctObjectDescriptorNative>(
                    IntPtr.Add(buffer, checked(index * elementSize)));
                if (native.ObjectId <= 0 || !Enum.IsDefined(typeof(OcctObjectKind), native.Kind))
                    throw new OcctException("Native object descriptor is invalid.");
                result[index] = new OcctObjectDescriptor(native.ObjectId, (OcctObjectKind)native.Kind);
            }
            return result;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public IReadOnlyList<IOcctObject> GetObjects() =>
        GetObjectDescriptors().Select(descriptor => CreateObject(descriptor.Id, descriptor.Kind)).ToArray();

    public void Delete(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Delete(new[] { value });
    }

    public void Delete(IEnumerable<IOcctObject> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        EnsureInitialized();
        var ids = GetObjectIds(values, nameof(values));
        if (ids.Length == 0) return;

        var buffer = Marshal.AllocHGlobal(sizeof(long) * ids.Length);
        try
        {
            Marshal.Copy(ids, 0, buffer, ids.Length);
            CheckObjectStatus(ObjectNativeMethods.occt_engine_objects_delete(_handle, buffer, ids.Length));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void Clear()
    {
        EnsureInitialized();
        CheckObjectStatus(ObjectNativeMethods.occt_engine_objects_clear(_handle));
    }

    internal IOcctObject CreateObject(long objectId, OcctObjectKind kind) => kind switch
    {
        OcctObjectKind.Shape => new OcctShape(objectId, _ownerId),
        OcctObjectKind.Text => new OcctText(objectId, _ownerId),
        OcctObjectKind.Dimension => new OcctDimension(objectId, _ownerId),
        OcctObjectKind.Point => new OcctPoint(objectId, _ownerId),
        OcctObjectKind.Overlay => new OcctOverlay(objectId, _ownerId),
        OcctObjectKind.Manipulator => new OcctManipulator(objectId, _ownerId),
        _ => throw new NotSupportedException($"Object kind {kind} is not supported by the managed bridge.")
    };

    private void GetObjectCounts(out int objectCount, out int shapeCount)
    {
        EnsureNotDisposed();
        CheckObjectStatus(ObjectNativeMethods.occt_engine_objects_snapshot_get(
            _handle,
            IntPtr.Zero,
            0,
            out objectCount,
            out shapeCount));
        if (objectCount < 0 || shapeCount < 0 || shapeCount > objectCount)
            throw new OcctException("Native object registry counts are invalid.");
    }

    private void UpdateObject(long objectId, NativeViewerObjectUpdateOptions options)
    {
        EnsureInitialized();
        CheckObjectStatus(ObjectNativeMethods.occt_engine_object_update(_handle, objectId, in options));
    }

    private void CheckObjectStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
