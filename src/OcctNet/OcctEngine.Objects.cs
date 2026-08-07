using System.Drawing;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public int ObjectCount
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.occt_object_count(_handle);
        }
    }

    public int ShapeCount
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.occt_shape_count(_handle);
        }
    }

    public IReadOnlyList<OcctObject> Objects
    {
        get
        {
            EnsureNotDisposed();
            return Enumerable.Range(0, ObjectCount)
                .Select(index => NativeMethods.occt_object_id_at(_handle, index))
                .Where(id => id > 0)
                .Select(id => new OcctObject(id, GetObjectKind(id), _ownerId))
                .ToArray();
        }
    }

    public IReadOnlyList<OcctShape> Shapes
    {
        get
        {
            EnsureNotDisposed();
            return Enumerable.Range(0, ShapeCount)
                .Select(index => NativeMethods.occt_shape_id_at(_handle, index))
                .Where(id => id > 0)
                .Select(id => new OcctShape(id, _ownerId))
                .ToArray();
        }
    }

    public bool Exists(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureNotDisposed();
        if (value.Id <= 0 || IsForeignObject(value)) return false;
        return NativeMethods.occt_object_exists(_handle, value.Id) != 0;
    }

    public bool Owns(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return GetOwnerId(value) == _ownerId;
    }

    /// <summary>Resolves a persisted native object ID into an engine-bound managed handle.</summary>
    public IOcctObject GetObject(long id)
    {
        EnsureNotDisposed();
        if (id <= 0 || NativeMethods.occt_object_exists(_handle, id) == 0)
            throw new ArgumentOutOfRangeException(nameof(id), id, "The object ID does not exist in this OCCT engine.");
        return CreateBoundObject(id, GetObjectKind(id));
    }

    public bool TryGetObject(long id, out IOcctObject? value)
    {
        EnsureNotDisposed();
        if (id > 0 && NativeMethods.occt_object_exists(_handle, id) != 0)
        {
            value = CreateBoundObject(id, GetObjectKind(id));
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>Resolves a persisted shape ID into an engine-bound shape handle.</summary>
    public OcctShape GetShape(long id)
    {
        EnsureNotDisposed();
        if (id <= 0 ||
            NativeMethods.occt_object_exists(_handle, id) == 0 ||
            NativeMethods.occt_object_kind(_handle, id) != (int)OcctObjectKind.Shape)
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "The shape ID does not exist in this OCCT engine.");
        }

        return new OcctShape(id, _ownerId);
    }

    public bool TryGetShape(long id, out OcctShape shape)
    {
        EnsureNotDisposed();
        if (id > 0 &&
            NativeMethods.occt_object_exists(_handle, id) != 0 &&
            NativeMethods.occt_object_kind(_handle, id) == (int)OcctObjectKind.Shape)
        {
            shape = new OcctShape(id, _ownerId);
            return true;
        }

        shape = default;
        return false;
    }

    public OcctObjectKind GetObjectKind(long id)
    {
        EnsureNotDisposed();
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        return (OcctObjectKind)NativeMethods.occt_object_kind(_handle, id);
    }

    public string GetName(IOcctObject value)
    {
        EnsureObject(value);
        return Marshal.PtrToStringUTF8(NativeMethods.occt_get_object_name(_handle, value.Id)) ?? string.Empty;
    }

    public void SetName(IOcctObject value, string name)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_set_object_name(_handle, value.Id, name ?? string.Empty));
    }

    public void SetColor(IOcctObject value, Color color)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_set_object_color(
            _handle,
            value.Id,
            color.R / 255.0,
            color.G / 255.0,
            color.B / 255.0));
    }

    public void SetTransparency(IOcctObject value, double transparency)
    {
        EnsureObject(value);
        OcctGuard.UnitInterval(transparency, nameof(transparency));
        CheckInitialized(() => NativeMethods.occt_set_object_transparency(_handle, value.Id, transparency));
    }

    public void SetVisible(IOcctObject value, bool visible)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_set_object_visible(_handle, value.Id, visible ? 1 : 0));
    }

    public void SetDisplayMode(IOcctObject value, OcctDisplayMode displayMode)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(displayMode)) throw new ArgumentOutOfRangeException(nameof(displayMode));
        CheckInitialized(() => NativeMethods.occt_set_object_display_mode(_handle, value.Id, (int)displayMode));
    }

    public void SetLineWidth(IOcctObject value, double width)
    {
        EnsureObject(value);
        OcctGuard.Positive(width, nameof(width));
        CheckInitialized(() => NativeMethods.occt_set_object_line_width(_handle, value.Id, width));
    }

    public void SetMaterial(IOcctObject value, OcctMaterial material)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(material)) throw new ArgumentOutOfRangeException(nameof(material));
        CheckInitialized(() => NativeMethods.occt_set_object_material(_handle, value.Id, (int)material));
    }

    public void Delete(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Delete(new[] { value });
    }

    public void Delete(IEnumerable<IOcctObject> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        EnsureInitialized();

        var ids = new HashSet<long>();
        foreach (var value in values)
        {
            ArgumentNullException.ThrowIfNull(value);
            EnsureObject(value);
            ids.Add(value.Id);
        }

        if (ids.Count == 0) return;
        var objectIds = ids.ToArray();
        Check(NativeMethods.occt_delete_objects(_handle, objectIds, objectIds.Length));
    }

    public void Clear() => CheckInitialized(() => NativeMethods.occt_clear(_handle));
    public void ShowAll() => CheckInitialized(() => NativeMethods.occt_show_all(_handle));
    public void HideAll() => CheckInitialized(() => NativeMethods.occt_hide_all(_handle));

    public void Redisplay(IOcctObject value)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_redisplay_object(_handle, value.Id));
    }

    public void Highlight(IOcctObject value)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_highlight_object(_handle, value.Id));
    }

    public void Unhighlight(IOcctObject value)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_unhighlight_object(_handle, value.Id));
    }

    public OcctShapeType GetShapeType(OcctShape shape)
    {
        EnsureShape(shape);
        return (OcctShapeType)NativeMethods.occt_shape_type(_handle, shape.Id);
    }

    public bool IsValid(OcctShape shape) =>
        Exists(shape) && NativeMethods.occt_shape_is_valid(_handle, shape.Id) != 0;

    public OcctBounds GetBounds(OcctShape shape)
    {
        EnsureShape(shape);
        EnsureInitialized();
        Check(NativeMethods.occt_shape_bounds(_handle, shape.Id, out var result));
        return result;
    }

    public OcctMassProperties GetLinearProperties(OcctShape shape) =>
        GetProperties(shape, NativeMethods.occt_shape_linear_properties);

    public OcctMassProperties GetSurfaceProperties(OcctShape shape) =>
        GetProperties(shape, NativeMethods.occt_shape_surface_properties);

    public OcctMassProperties GetVolumeProperties(OcctShape shape) =>
        GetProperties(shape, NativeMethods.occt_shape_volume_properties);

    public OcctDistanceResult Distance(OcctShape first, OcctShape second)
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

    public OcctShape GetSubshape(OcctShape shape, OcctShapeType type, int index)
    {
        EnsureShape(shape);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        OcctGuard.PositiveIndex(index, nameof(index));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_get_subshape(_handle, shape.Id, (int)type, index));
    }

    public IReadOnlyList<OcctShape> GetSubshapes(OcctShape shape, OcctShapeType type) =>
        Enumerable.Range(0, GetTopologyCount(shape, type))
            .Select(index => GetSubshape(shape, type, index))
            .ToArray();

    public OcctShape Copy(OcctShape shape, bool hideInput = false)
    {
        EnsureShape(shape);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_copy_shape(_handle, shape.Id, hideInput ? 1 : 0));
    }

    public OcctShape Translate(OcctShape shape, OcctVector3d vector, bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.Finite(vector, nameof(vector));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_translate(_handle, shape.Id, vector, hideInput ? 1 : 0));
    }

    public OcctShape Rotate(
        OcctShape shape,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.Finite(axisPoint, nameof(axisPoint));
        OcctGuard.NonZero(axisDirection, nameof(axisDirection));
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_rotate(
            _handle,
            shape.Id,
            axisPoint,
            axisDirection,
            angleDegrees,
            hideInput ? 1 : 0));
    }

    public OcctShape Scale(OcctShape shape, OcctPoint3d center, double factor, bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.Finite(center, nameof(center));
        OcctGuard.Positive(factor, nameof(factor));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_scale(_handle, shape.Id, center, factor, hideInput ? 1 : 0));
    }

    public OcctShape MirrorPlane(
        OcctShape shape,
        OcctPoint3d planePoint,
        OcctVector3d planeNormal,
        bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.Finite(planePoint, nameof(planePoint));
        OcctGuard.NonZero(planeNormal, nameof(planeNormal));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_mirror_plane(
            _handle,
            shape.Id,
            planePoint,
            planeNormal,
            hideInput ? 1 : 0));
    }

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

    public OcctCurveType GetCurveType(OcctShape edge)
    {
        EnsureShape(edge);
        return (OcctCurveType)NativeMethods.occt_edge_curve_type(_handle, edge.Id);
    }

    public OcctSurfaceType GetSurfaceType(OcctShape face)
    {
        EnsureShape(face);
        return (OcctSurfaceType)NativeMethods.occt_face_surface_type(_handle, face.Id);
    }

    public OcctUvBounds GetUvBounds(OcctShape face)
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

    private IOcctObject CreateBoundObject(long id, OcctObjectKind kind) => kind switch
    {
        OcctObjectKind.Shape => new OcctShape(id, _ownerId),
        OcctObjectKind.Text => new OcctText(id, _ownerId),
        OcctObjectKind.Dimension => new OcctDimension(id, _ownerId),
        _ => new OcctObject(id, kind, _ownerId)
    };

    private delegate int PropertyCall(IntPtr handle, long id, out OcctMassProperties result);

    private OcctMassProperties GetProperties(OcctShape shape, PropertyCall call)
    {
        EnsureShape(shape);
        EnsureInitialized();
        Check(call(_handle, shape.Id, out var result));
        return result;
    }
}
