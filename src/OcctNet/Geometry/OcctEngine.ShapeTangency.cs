namespace OcctNet;

public sealed partial class OcctEngine
{
    public IReadOnlyList<OcctEdgeTangentPoint> GetTangentPoints(
        OcctShape edge,
        OcctPlane3d plane,
        OcctPoint3d sourcePoint,
        double linearTolerance = 1e-7,
        double angularTolerance = 1e-7)
    {
        EnsureShape(edge);
        if (!plane.Origin.IsFinite)
            throw new ArgumentOutOfRangeException(
                nameof(plane),
                "Tangent plane origin must be finite.");
        if (!plane.Normal.TryNormalize(out var normal))
            throw new ArgumentOutOfRangeException(
                nameof(plane),
                "Tangent plane normal must be finite and non-zero.");
        if (!sourcePoint.IsFinite)
            throw new ArgumentOutOfRangeException(
                nameof(sourcePoint),
                "Tangent source point must be finite.");
        if (!double.IsFinite(linearTolerance) ||
            linearTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(linearTolerance),
                "Tangent linear tolerance must be finite and non-negative.");
        }
        if (!double.IsFinite(angularTolerance) ||
            angularTolerance <= 0.0 ||
            angularTolerance > Math.PI * 0.5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(angularTolerance),
                "Tangent angular tolerance must be in (0, pi/2].");
        }

        EnsureInitialized();

        var status =
            ViewerShapeNativeMethods
                .occt_engine_shape_edge_tangent_points_snapshot_get(
                    _handle,
                    edge.Id,
                    plane.Origin,
                    normal,
                    sourcePoint,
                    linearTolerance,
                    angularTolerance,
                    null,
                    0,
                    out var count);
        if (status != OcctStatus.Ok)
            throw CreateException();
        if (count == 0)
            return Array.Empty<OcctEdgeTangentPoint>();
        if (count < 0)
            throw new InvalidOperationException(
                "Native tangent-point count is invalid.");

        var native = new NativeEdgeTangentPoint[count];
        status =
            ViewerShapeNativeMethods
                .occt_engine_shape_edge_tangent_points_snapshot_get(
                    _handle,
                    edge.Id,
                    plane.Origin,
                    normal,
                    sourcePoint,
                    linearTolerance,
                    angularTolerance,
                    native,
                    native.Length,
                    out var required);
        if (status != OcctStatus.Ok)
            throw CreateException();
        if (required != count)
        {
            throw new InvalidOperationException(
                "Native tangent-point count changed during snapshot copy.");
        }

        var result = new OcctEdgeTangentPoint[count];
        for (var index = 0; index < count; index++)
        {
            var value = native[index].ToManaged();
            if (!value.Point.IsFinite ||
                !double.IsFinite(value.NormalizedParameter) ||
                value.NormalizedParameter < 0.0 ||
                value.NormalizedParameter > 1.0)
            {
                throw new InvalidOperationException(
                    "Native tangent-point query returned invalid geometry.");
            }

            result[index] = value;
        }

        return result;
    }
}
