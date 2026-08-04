using System.Runtime.InteropServices;

namespace OcctNet;

/// <summary>
/// Headless OCCT modeling session. No HWND, AIS context, or viewer is required.
/// </summary>
public sealed partial class OcctModelingSession : IDisposable
{
    private IntPtr _handle;

    public OcctModelingSession()
    {
        OcctRuntime.Configure();
        _ = NativeMethods.occt_version();
        _handle = ModelNativeMethods.occt_model_create();
        if (_handle == IntPtr.Zero)
            throw new OcctException("Unable to create the native OCCT modeling session.");
    }

    internal IntPtr NativeHandle
    {
        get
        {
            EnsureNotDisposed();
            return _handle;
        }
    }

    public static string Capabilities =>
        Marshal.PtrToStringUTF8(ModelNativeMethods.occt_model_capabilities()) ?? string.Empty;

    public int ShapeCount
    {
        get
        {
            EnsureNotDisposed();
            return ModelNativeMethods.occt_model_shape_count(_handle);
        }
    }

    public IReadOnlyList<OcctModelShape> Shapes
    {
        get
        {
            EnsureNotDisposed();
            return Enumerable.Range(0, ShapeCount)
                .Select(index => ModelNativeMethods.occt_model_shape_id_at(_handle, index))
                .Where(id => id > 0)
                .Select(id => new OcctModelShape(id))
                .ToArray();
        }
    }

    public bool Exists(OcctModelShape shape)
    {
        EnsureNotDisposed();
        return ModelNativeMethods.occt_model_shape_exists(_handle, shape.Id) != 0;
    }

    public void Delete(OcctModelShape shape) => Check(ModelNativeMethods.occt_model_delete_shape(NativeHandle, shape.Id));
    public void Clear() => Check(ModelNativeMethods.occt_model_clear(NativeHandle));
    public OcctModelShape Copy(OcctModelShape shape) => CheckShape(ModelNativeMethods.occt_model_copy_shape(NativeHandle, shape.Id));

    public long GetShapeHash(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_hash(_handle, shape.Id);
    }

    public OcctShapeType GetShapeType(OcctModelShape shape)
    {
        EnsureShape(shape);
        return (OcctShapeType)ModelNativeMethods.occt_model_shape_type(_handle, shape.Id);
    }

    public OcctModelOrientation GetOrientation(OcctModelShape shape)
    {
        EnsureShape(shape);
        return (OcctModelOrientation)ModelNativeMethods.occt_model_shape_orientation(_handle, shape.Id);
    }

    public bool IsClosed(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_is_closed(_handle, shape.Id) != 0;
    }

    public bool IsValid(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_is_valid(_handle, shape.Id) != 0;
    }

    public double GetMaximumTolerance(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_tolerance(_handle, shape.Id);
    }

    public string GetCheckReport(OcctModelShape shape)
    {
        EnsureShape(shape);
        return Marshal.PtrToStringUTF8(ModelNativeMethods.occt_model_check_report(_handle, shape.Id)) ?? string.Empty;
    }

    public OcctBounds GetBounds(OcctModelShape shape)
    {
        EnsureShape(shape);
        Check(ModelNativeMethods.occt_model_shape_bounds(_handle, shape.Id, out var result));
        return result;
    }

    public OcctMassProperties GetLinearProperties(OcctModelShape shape) => GetProperties(shape, ModelNativeMethods.occt_model_shape_linear_properties);
    public OcctMassProperties GetSurfaceProperties(OcctModelShape shape) => GetProperties(shape, ModelNativeMethods.occt_model_shape_surface_properties);
    public OcctMassProperties GetVolumeProperties(OcctModelShape shape) => GetProperties(shape, ModelNativeMethods.occt_model_shape_volume_properties);

    public OcctDistanceResult Distance(OcctModelShape first, OcctModelShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        Check(ModelNativeMethods.occt_model_shape_distance(_handle, first.Id, second.Id, out var result));
        return result;
    }

    public OcctModelLocation GetLocation(OcctModelShape shape)
    {
        EnsureShape(shape);
        Check(ModelNativeMethods.occt_model_get_location(_handle, shape.Id, out var result));
        return result;
    }

    public OcctModelShape SetLocation(OcctModelShape shape, OcctModelLocation location, bool copyShape = true)
    {
        EnsureShape(shape);
        return CheckShape(ModelNativeMethods.occt_model_set_location(_handle, shape.Id, in location, copyShape ? 1 : 0));
    }

    public int GetTopologyCount(OcctModelShape shape, OcctShapeType type)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_topology_count(_handle, shape.Id, (int)type);
    }

    public OcctModelShape GetSubshape(OcctModelShape shape, OcctShapeType type, int index)
    {
        EnsureShape(shape);
        return CheckShape(ModelNativeMethods.occt_model_get_subshape(_handle, shape.Id, (int)type, index));
    }

    public IReadOnlyList<OcctModelShape> GetSubshapes(OcctModelShape shape, OcctShapeType type) =>
        Enumerable.Range(0, GetTopologyCount(shape, type))
            .Select(index => GetSubshape(shape, type, index))
            .ToArray();

    public OcctModelShape GetOuterWire(OcctModelShape face)
    {
        EnsureShape(face);
        return CheckShape(ModelNativeMethods.occt_model_outer_wire(_handle, face.Id));
    }

    public IReadOnlyList<OcctModelShape> GetInnerWires(OcctModelShape face)
    {
        EnsureShape(face);
        var count = ModelNativeMethods.occt_model_inner_wire_count(_handle, face.Id);
        return Enumerable.Range(0, count)
            .Select(index => CheckShape(ModelNativeMethods.occt_model_inner_wire_at(_handle, face.Id, index)))
            .ToArray();
    }

    public IReadOnlyList<OcctModelShape> GetAncestors(OcctModelShape root, OcctModelShape child, OcctShapeType ancestorType)
    {
        EnsureShape(root);
        EnsureShape(child);
        var count = ModelNativeMethods.occt_model_ancestor_count(_handle, root.Id, child.Id, (int)ancestorType);
        return Enumerable.Range(0, count)
            .Select(index => CheckShape(ModelNativeMethods.occt_model_ancestor_at(_handle, root.Id, child.Id, (int)ancestorType, index)))
            .ToArray();
    }

    public OcctPoint3d GetVertexPoint(OcctModelShape vertex)
    {
        EnsureShape(vertex);
        Check(ModelNativeMethods.occt_model_vertex_point(_handle, vertex.Id, out var result));
        return result;
    }

    public (OcctPoint3d Start, OcctPoint3d End) GetEdgeEndpoints(OcctModelShape edge)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_endpoints(_handle, edge.Id, out var start, out var end));
        return (start, end);
    }

    public OcctEdgeEvaluation EvaluateEdge(OcctModelShape edge, double normalizedParameter)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_point_at(_handle, edge.Id, normalizedParameter, out var point, out var tangent));
        return new OcctEdgeEvaluation(point, tangent);
    }

    public OcctCurveType GetCurveType(OcctModelShape edge)
    {
        EnsureShape(edge);
        return (OcctCurveType)ModelNativeMethods.occt_model_edge_curve_type(_handle, edge.Id);
    }

    public OcctSurfaceType GetSurfaceType(OcctModelShape face)
    {
        EnsureShape(face);
        return (OcctSurfaceType)ModelNativeMethods.occt_model_face_surface_type(_handle, face.Id);
    }

    public OcctUvBounds GetUvBounds(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_uv_bounds(_handle, face.Id, out var result));
        return result;
    }

    public OcctFaceEvaluation EvaluateFace(OcctModelShape face, double u, double v)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_point_normal(_handle, face.Id, u, v, out var point, out var normal));
        return new OcctFaceEvaluation(point, normal);
    }

    private delegate long ImportCall(IntPtr handle, string path);
    private OcctModelShape ImportSpecific(string filePath, ImportCall call)
    {
        ValidatePath(filePath);
        return CheckShape(call(_handle, Path.GetFullPath(filePath)));
    }

    private delegate int ExportCall(IntPtr handle, long shapeId, string path);
    private void ExportShape(OcctModelShape shape, string filePath, ExportCall call)
    {
        EnsureShape(shape);
        ValidatePath(filePath);
        Check(call(_handle, shape.Id, Path.GetFullPath(filePath)));
    }

    private static void ValidatePath(string path) => ArgumentException.ThrowIfNullOrWhiteSpace(path);

    private delegate int PropertyCall(IntPtr handle, long id, out OcctMassProperties result);

    private OcctMassProperties GetProperties(OcctModelShape shape, PropertyCall call)
    {
        EnsureShape(shape);
        Check(call(_handle, shape.Id, out var result));
        return result;
    }

    private static T[] RequiredArray<T>(IEnumerable<T> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var result = values.ToArray();
        if (result.Length == 0) throw new ArgumentException("Collection must not be empty.", parameterName);
        return result;
    }

    private long[] ShapeIds(IEnumerable<OcctModelShape> shapes)
    {
        var array = RequiredArray(shapes, nameof(shapes));
        foreach (var shape in array) EnsureShape(shape);
        return array.Select(shape => shape.Id).ToArray();
    }

    private void EnsureShape(OcctModelShape shape)
    {
        EnsureNotDisposed();
        if (!shape.IsValid || ModelNativeMethods.occt_model_shape_exists(_handle, shape.Id) == 0)
            throw new ArgumentException("Shape does not belong to this modeling session.", nameof(shape));
    }

    private OcctModelShape CheckShape(long id)
    {
        if (id <= 0) throw CreateException();
        return new OcctModelShape(id);
    }

    private OcctModelAlgorithmResult CheckAlgorithm(NativeModelAlgorithmResult native)
    {
        if (native.Succeeded == 0 || native.ShapeId <= 0) throw CreateException();
        return new OcctModelAlgorithmResult(this, native);
    }

    private void Check(int result)
    {
        if (result == 0) throw CreateException();
    }

    private OcctException CreateException()
    {
        var pointer = _handle == IntPtr.Zero ? IntPtr.Zero : ModelNativeMethods.occt_model_last_error(_handle);
        var message = pointer == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(pointer);
        return new OcctException(string.IsNullOrWhiteSpace(message)
            ? "The native OCCT modeling operation failed."
            : message);
    }

    private void EnsureNotDisposed() => ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

    public void Dispose()
    {
        if (_handle == IntPtr.Zero) return;
        ModelNativeMethods.occt_model_destroy(_handle);
        _handle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    ~OcctModelingSession() => Dispose();
}
