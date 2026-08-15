namespace OcctNet;

public sealed partial class OcctModelingSession
{
    /// <summary>
    /// Creates an owned native snapshot that remains valid after its source registry entry is deleted.
    /// </summary>
    public OcctShapeResource AcquireShape(OcctModelShape shape)
    {
        EnsureShape(shape);
        var status = ModelNativeMethods.occt_model_shape_acquire(_handle, shape.Id, out var nativeHandle);
        if (status != OcctStatus.Ok)
            throw CreateException(nameof(AcquireShape));

        var safeHandle = OcctShapeSafeHandle.AdoptOwned(nativeHandle);
        if (safeHandle.IsInvalid)
        {
            safeHandle.Dispose();
            throw new OcctException(
                "Native shape acquisition returned an invalid handle.",
                OcctStatus.ErrorUnknown,
                nameof(AcquireShape));
        }
        return new OcctShapeResource(safeHandle);
    }
}
