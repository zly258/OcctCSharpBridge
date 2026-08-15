namespace OcctNet;

public sealed partial class OcctEngine
{
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
        if (!Enum.IsDefined(options))
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
