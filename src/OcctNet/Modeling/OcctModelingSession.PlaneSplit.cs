namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctPlaneSplitResult SplitByPlane(OcctModelShape shape, OcctPlane3d plane)
    {
        EnsureShape(shape);
        OcctGuard.Finite(plane.Origin, nameof(plane));
        OcctGuard.NonZero(plane.Normal, nameof(plane));
        CheckStatus(ModelNativeMethods.occt_model_split_by_plane(
            _handle, shape.Id, plane.Origin, plane.Normal, out var native));

        OcctModelShape? ToShape(long id) =>
            id > 0 ? new OcctModelShape(id, _ownerId) : null;

        return new OcctPlaneSplitResult(
            ToShape(native.PositiveShapeId),
            ToShape(native.NegativeShapeId),
            ToShape(native.SectionShapeId));
    }
}
