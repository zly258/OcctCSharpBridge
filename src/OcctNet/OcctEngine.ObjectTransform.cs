namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetLocalTransformation(IOcctObject value, OcctTransform3d transform)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureInitialized();
        Check(NativeMethods.occt_set_object_transform(_handle, value.Id, transform.ToArray()));
    }

    public OcctTransform3d GetLocalTransformation(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureInitialized();
        var matrix = new double[12];
        Check(NativeMethods.occt_get_object_transform(_handle, value.Id, matrix, out _));
        return OcctTransform3d.FromArray(matrix);
    }

    public bool HasLocalTransformation(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureInitialized();
        var matrix = new double[12];
        Check(NativeMethods.occt_get_object_transform(_handle, value.Id, matrix, out var hasTransform));
        return hasTransform != 0;
    }

    public void ResetLocalTransformation(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        CheckInitialized(() => NativeMethods.occt_reset_object_transform(_handle, value.Id));
    }

    public void SetLocalTransformations(IReadOnlyList<OcctObjectTransformUpdate> updates)
    {
        ArgumentNullException.ThrowIfNull(updates);
        EnsureInitialized();
        using var batch = BeginDisplayBatch();
        foreach (var update in updates)
        {
            ArgumentNullException.ThrowIfNull(update.Object);
            Check(NativeMethods.occt_set_object_transform(_handle, update.Object.Id, update.Transformation.ToArray()));
        }
    }
}
