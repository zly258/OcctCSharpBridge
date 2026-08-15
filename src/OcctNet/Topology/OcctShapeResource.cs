namespace OcctNet;

/// <summary>
/// Owns an immutable native shape snapshot independently from a modeling session registry.
/// </summary>
public sealed class OcctShapeResource : IDisposable
{
    private readonly OcctShapeSafeHandle _handle;

    internal OcctShapeResource(OcctShapeSafeHandle handle)
    {
        _handle = handle;
    }

    public bool IsDisposed => _handle.IsClosed || _handle.IsInvalid;

    public OcctShapeType ShapeType
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            var status = ModelNativeMethods.occt_shape_get_type(_handle, out var result);
            if (status != OcctStatus.Ok)
                throw new OcctException("Unable to query the owned native shape.", status, nameof(ShapeType));
            return (OcctShapeType)result;
        }
    }

    public void Dispose()
    {
        _handle.Dispose();
    }
}
