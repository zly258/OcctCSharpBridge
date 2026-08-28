namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctHlrResult ProjectHlr(OcctModelShape shape, OcctHlrProjection projection)
    {
        EnsureShape(shape);
        OcctGuard.NonZero(projection.ViewDirection, nameof(projection));
        OcctGuard.NonZero(projection.UpDirection, nameof(projection));
        CheckStatus(ModelNativeMethods.occt_model_hlr_project(
            _handle,
            shape.Id,
            projection.ViewDirection,
            projection.UpDirection,
            out var native));

        OcctModelShape? ToShape(long id) =>
            id > 0 ? new OcctModelShape(id, _ownerId) : null;

        return new OcctHlrResult(
            ToShape(native.VisibleShapeId),
            ToShape(native.HiddenShapeId),
            ToShape(native.OutlineShapeId),
            ToShape(native.VisibleSharpShapeId),
            ToShape(native.HiddenSharpShapeId));
    }
}
