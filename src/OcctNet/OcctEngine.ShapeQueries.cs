using System.ComponentModel;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShapeType GetShapeType(OcctShape shape)
    {
        EnsureShape(shape);
        return (OcctShapeType)NativeMethods.occt_shape_type(_handle, shape.Id);
    }

    public bool IsShapeValid(OcctShape shape) =>
        Exists(shape) && NativeMethods.occt_shape_is_valid(_handle, shape.Id) != 0;

    public OcctBounds GetShapeBounds(OcctShape shape)
    {
        EnsureShape(shape);
        EnsureInitialized();
        Check(NativeMethods.occt_shape_bounds(_handle, shape.Id, out var result));
        return result;
    }

    /// <summary>
    /// Bridge 2.5 source-compatibility entry point. New code should use
    /// <see cref="GetShapeBounds(OcctShape)"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public OcctBounds GetBounds(OcctShape shape) => GetShapeBounds(shape);

    public OcctMassProperties GetShapeLinearProperties(OcctShape shape) =>
        GetProperties(shape, NativeMethods.occt_shape_linear_properties);

    public OcctMassProperties GetShapeSurfaceProperties(OcctShape shape) =>
        GetProperties(shape, NativeMethods.occt_shape_surface_properties);

    public OcctMassProperties GetShapeVolumeProperties(OcctShape shape) =>
        GetProperties(shape, NativeMethods.occt_shape_volume_properties);

    public OcctDistanceResult GetShapeDistance(OcctShape first, OcctShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        EnsureInitialized();
        Check(NativeMethods.occt_shape_distance(_handle, first.Id, second.Id, out var result));
        return result;
    }

    public int GetTopologyCount(OcctShape shape, OcctShapeType type)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        return NativeMethods.occt_topology_count(_handle, shape.Id, (int)type);
    }

    public OcctShape GetSubshapeAt(OcctShape shape, OcctShapeType type, int index)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        OcctGuard.PositiveIndex(index, nameof(index));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_get_subshape(_handle, shape.Id, (int)type, index));
    }

    public IReadOnlyList<OcctShape> GetSubshapes(OcctShape shape, OcctShapeType type) =>
        Enumerable.Range(0, GetTopologyCount(shape, type))
            .Select(index => GetSubshapeAt(shape, type, index))
            .ToArray();

    public long GetShapeHash(OcctShape shape)
    {
        EnsureShape(shape);
        return NativeMethods.occt_shape_hash(_handle, shape.Id);
    }

    public OcctPoint3d GetVertexPoint(OcctShape vertex)
    {
        EnsureShape(vertex);
        EnsureInitialized();
        Check(NativeMethods.occt_vertex_point(_handle, vertex.Id, out var result));
        return result;
    }

    public (OcctPoint3d Start, OcctPoint3d End) GetEdgeEndpoints(OcctShape edge)
    {
        EnsureShape(edge);
        EnsureInitialized();
        Check(NativeMethods.occt_edge_endpoints(_handle, edge.Id, out var start, out var end));
        return (start, end);
    }

    public OcctEdgeEvaluation EvaluateEdge(OcctShape edge, double normalizedParameter)
    {
        EnsureShape(edge);
        OcctGuard.UnitInterval(normalizedParameter, nameof(normalizedParameter));
        EnsureInitialized();
        Check(NativeMethods.occt_edge_point_at(_handle, edge.Id, normalizedParameter, out var point, out var tangent));
        return new(point, tangent);
    }

    public OcctCurveType GetEdgeCurveType(OcctShape edge)
    {
        EnsureShape(edge);
        return (OcctCurveType)NativeMethods.occt_edge_curve_type(_handle, edge.Id);
    }

    public OcctSurfaceType GetFaceSurfaceType(OcctShape face)
    {
        EnsureShape(face);
        return (OcctSurfaceType)NativeMethods.occt_face_surface_type(_handle, face.Id);
    }

    public OcctUvBounds GetFaceUvBounds(OcctShape face)
    {
        EnsureShape(face);
        EnsureInitialized();
        Check(NativeMethods.occt_face_uv_bounds(_handle, face.Id, out var result));
        return result;
    }

    public OcctFaceEvaluation EvaluateFace(OcctShape face, double u, double v)
    {
        EnsureShape(face);
        OcctGuard.Finite(u, nameof(u));
        OcctGuard.Finite(v, nameof(v));
        EnsureInitialized();
        Check(NativeMethods.occt_face_point_normal(_handle, face.Id, u, v, out var point, out var normal));
        return new(point, normal);
    }

    private delegate int PropertyCall(IntPtr handle, long id, out OcctMassProperties result);

    private OcctMassProperties GetProperties(OcctShape shape, PropertyCall call)
    {
        EnsureShape(shape);
        EnsureInitialized();
        Check(call(_handle, shape.Id, out var result));
        return result;
    }
}
