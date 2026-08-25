namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape CreateShapeFromModel(
        OcctModelingSession sourceSession,
        OcctModelShape sourceShape)
    {
        ArgumentNullException.ThrowIfNull(sourceSession);
        if (!sourceSession.Exists(sourceShape))
            throw new ArgumentException("Shape does not belong to the supplied modeling session.", nameof(sourceShape));

        EnsureInitialized();
        var status = ViewerModelInteropNativeMethods.occt_engine_object_shape_create_from_model(
            _handle,
            sourceSession.NativeHandle,
            sourceShape.Id,
            out var viewerObjectId);
        if (status != OcctStatus.Ok) throw CreateException();
        return CheckShape(viewerObjectId);
    }

    public void UpdateShape(
        OcctShape viewerShape,
        OcctModelingSession sourceSession,
        OcctModelShape sourceShape,
        OcctShapeUpdateOptions options = OcctShapeUpdateOptions.PreserveAll)
    {
        ArgumentNullException.ThrowIfNull(sourceSession);
        EnsureShape(viewerShape);
        if (!sourceSession.Exists(sourceShape))
            throw new ArgumentException("Shape does not belong to the supplied modeling session.", nameof(sourceShape));
        if ((options & ~OcctShapeUpdateOptions.PreserveAll) != 0)
            throw new ArgumentOutOfRangeException(nameof(options));

        EnsureInitialized();
        var status = ViewerModelInteropNativeMethods.occt_engine_object_shape_update_from_model(
            _handle,
            sourceSession.NativeHandle,
            viewerShape.Id,
            sourceShape.Id,
            (uint)options);
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
