namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShapeType GetShapeType(OcctShape shape)
    {
        EnsureShape(shape);
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_type_get(_handle, shape.Id, out var value);
        if (status != OcctStatus.Ok) throw CreateException();
        if (!Enum.IsDefined(typeof(OcctShapeType), value))
            throw new InvalidOperationException($"Native shape type {value} is not supported by this SDK.");
        return (OcctShapeType)value;
    }

    public bool IsShapeValid(OcctShape shape)
    {
        EnsureNotDisposed();
        if (!shape.IsValid || shape.OwnerId != _ownerId || !ObjectExists(shape.Id)) return false;
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_validity_get(_handle, shape.Id, out var value);
        if (status != OcctStatus.Ok) throw CreateException();
        if (value is not 0 and not 1)
            throw new InvalidOperationException("Native shape validity state is invalid.");
        return value != 0;
    }

    public OcctBounds GetShapeBounds(OcctShape shape)
    {
        EnsureShape(shape);
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_bounds_get(_handle, shape.Id, out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return result;
    }

    public OcctMassProperties GetShapeLinearProperties(OcctShape shape) =>
        GetProperties(shape, ViewerShapeNativeMethods.occt_engine_shape_linear_properties_get);

    public OcctMassProperties GetShapeSurfaceProperties(OcctShape shape) =>
        GetProperties(shape, ViewerShapeNativeMethods.occt_engine_shape_surface_properties_get);

    public OcctMassProperties GetShapeVolumeProperties(OcctShape shape) =>
        GetProperties(shape, ViewerShapeNativeMethods.occt_engine_shape_volume_properties_get);

    public OcctDistanceResult GetShapeDistance(OcctShape first, OcctShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_distance_get(
            _handle,
            first.Id,
            second.Id,
            out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return result;
    }

    public int GetTopologyCount(OcctShape shape, OcctShapeType type)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_topology_count_get(
            _handle,
            shape.Id,
            (int)type,
            out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return result;
    }

    public OcctShape GetSubshapeAt(OcctShape shape, OcctShapeType type, int index)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        OcctGuard.PositiveIndex(index, nameof(index));
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_subshape_copy(
            _handle,
            shape.Id,
            (int)type,
            index,
            out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return CheckShape(result);
    }

    public IReadOnlyList<OcctShape> GetSubshapes(OcctShape shape, OcctShapeType type) =>
        Enumerable.Range(0, GetTopologyCount(shape, type))
            .Select(index => GetSubshapeAt(shape, type, index))
            .ToArray();

    public long GetShapeHash(OcctShape shape)
    {
        EnsureShape(shape);
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_hash_get(_handle, shape.Id, out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return result;
    }

    public OcctPoint3d GetVertexPoint(OcctShape vertex)
    {
        EnsureShape(vertex);
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_vertex_point_get(_handle, vertex.Id, out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return result;
    }

    public (OcctPoint3d Start, OcctPoint3d End) GetEdgeEndpoints(OcctShape edge)
    {
        EnsureShape(edge);
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_edge_endpoints_get(
            _handle,
            edge.Id,
            out var start,
            out var end);
        if (status != OcctStatus.Ok) throw CreateException();
        return (start, end);
    }

    public OcctEdgeEvaluation EvaluateEdge(OcctShape edge, double normalizedParameter)
    {
        EnsureShape(edge);
        OcctGuard.UnitInterval(normalizedParameter, nameof(normalizedParameter));
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_edge_evaluate(
            _handle,
            edge.Id,
            normalizedParameter,
            out var point,
            out var tangent);
        if (status != OcctStatus.Ok) throw CreateException();
        return new(point, tangent);
    }

    public OcctCurveType GetEdgeCurveType(OcctShape edge)
    {
        EnsureShape(edge);
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_edge_curve_type_get(_handle, edge.Id, out var value);
        if (status != OcctStatus.Ok) throw CreateException();
        if (!Enum.IsDefined(typeof(OcctCurveType), value))
            throw new InvalidOperationException($"Native curve type {value} is not supported by this SDK.");
        return (OcctCurveType)value;
    }

    public OcctSurfaceType GetFaceSurfaceType(OcctShape face)
    {
        EnsureShape(face);
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_face_surface_type_get(_handle, face.Id, out var value);
        if (status != OcctStatus.Ok) throw CreateException();
        if (!Enum.IsDefined(typeof(OcctSurfaceType), value))
            throw new InvalidOperationException($"Native surface type {value} is not supported by this SDK.");
        return (OcctSurfaceType)value;
    }

    public OcctUvBounds GetFaceUvBounds(OcctShape face)
    {
        EnsureShape(face);
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_face_uv_bounds_get(_handle, face.Id, out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return result;
    }

    public OcctFaceEvaluation EvaluateFace(OcctShape face, double u, double v)
    {
        EnsureShape(face);
        OcctGuard.Finite(u, nameof(u));
        OcctGuard.Finite(v, nameof(v));
        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_shape_face_evaluate(
            _handle,
            face.Id,
            u,
            v,
            out var point,
            out var normal);
        if (status != OcctStatus.Ok) throw CreateException();
        return new(point, normal);
    }

    private delegate OcctStatus PropertyCall(
        OcctEngineSafeHandle handle,
        long id,
        out OcctMassProperties result);

    private OcctMassProperties GetProperties(OcctShape shape, PropertyCall call)
    {
        EnsureShape(shape);
        EnsureInitialized();
        var status = call(_handle, shape.Id, out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return result;
    }
}
