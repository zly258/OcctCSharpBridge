using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetLocalTransformation(IOcctObject value, OcctTransform3d transform)
    {
        EnsureObject(value);
        if (!transform.IsFinite)
            throw new ArgumentException("Transformation matrix must contain only finite values.", nameof(transform));
        EnsureInitialized();
        CheckObjectTransformStatus(ObjectTransformNativeMethods.occt_engine_object_transform_set(
            _handle,
            value.Id,
            in transform));
    }

    public OcctTransform3d GetLocalTransformation(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckObjectTransformStatus(ObjectTransformNativeMethods.occt_engine_object_transform_get(
            _handle,
            value.Id,
            out var transformation,
            out _));
        return transformation;
    }

    public bool HasLocalTransformation(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckObjectTransformStatus(ObjectTransformNativeMethods.occt_engine_object_transform_get(
            _handle,
            value.Id,
            out _,
            out var hasTransformation));
        if (hasTransformation is not 0 and not 1)
            throw new InvalidOperationException("Native transformation state is invalid.");
        return hasTransformation != 0;
    }

    public void ResetLocalTransformation(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckObjectTransformStatus(ObjectTransformNativeMethods.occt_engine_object_transform_reset(
            _handle,
            value.Id));
    }

    public void SetLocalTransformations(IReadOnlyList<OcctObjectTransformUpdate> updates)
    {
        ArgumentNullException.ThrowIfNull(updates);
        EnsureInitialized();
        if (updates.Count == 0) return;

        var native = new NativeViewerObjectTransformUpdate[updates.Count];
        var objectIds = new HashSet<long>();
        for (var index = 0; index < updates.Count; index++)
        {
            var update = updates[index];
            ArgumentNullException.ThrowIfNull(update.Object);
            EnsureObject(update.Object);
            if (!update.Transformation.IsFinite)
                throw new ArgumentException("Transformation matrix must contain only finite values.", nameof(updates));
            if (!objectIds.Add(update.Object.Id))
                throw new ArgumentException("Transformation updates must contain unique objects.", nameof(updates));

            native[index] = new NativeViewerObjectTransformUpdate
            {
                ObjectId = update.Object.Id,
                Transformation = update.Transformation
            };
        }

        var pinned = GCHandle.Alloc(native, GCHandleType.Pinned);
        try
        {
            CheckObjectTransformStatus(ObjectTransformNativeMethods.occt_engine_object_transforms_set(
                _handle,
                pinned.AddrOfPinnedObject(),
                native.Length));
        }
        finally
        {
            pinned.Free();
        }
    }

    private void CheckObjectTransformStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
