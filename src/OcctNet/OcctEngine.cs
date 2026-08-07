using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace OcctNet;

public sealed partial class OcctEngine : IDisposable
{
    private static long s_nextOwnerId;

    private readonly long _ownerId = Interlocked.Increment(ref s_nextOwnerId);
    private IntPtr _handle;
    private bool _initialized;

    public OcctEngine()
    {
        OcctRuntime.Configure();
        OcctBridgeInfo.EnsureCompatible();
        _handle = NativeMethods.occt_create();
        if (_handle == IntPtr.Zero)
            throw new OcctException("Unable to create the native OCCT engine.", nameof(OcctEngine));
    }

    internal long OwnerId => _ownerId;

    public bool IsInitialized => Volatile.Read(ref _initialized) && Volatile.Read(ref _handle) != IntPtr.Zero;
    public static string OcctVersion => OcctBridgeInfo.OcctVersion;
    public int ObjectCount { get { EnsureNotDisposed(); return NativeMethods.occt_object_count(_handle); } }
    public int ShapeCount { get { EnsureNotDisposed(); return NativeMethods.occt_shape_count(_handle); } }

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

    public void Initialize(IntPtr windowHandle)
    {
        EnsureNotDisposed();
        if (windowHandle == IntPtr.Zero) throw new ArgumentException("Window handle must not be zero.", nameof(windowHandle));
        if (Volatile.Read(ref _initialized)) return;
        Check(NativeMethods.occt_initialize(_handle, windowHandle));
        Volatile.Write(ref _initialized, true);
    }

    public void Resize() => CheckInitialized(() => NativeMethods.occt_resize(_handle));
    public void Redraw() => CheckInitialized(() => NativeMethods.occt_redraw(_handle));
    public void FitAll() => CheckInitialized(() => NativeMethods.occt_fit_all(_handle));
    public void Fit(OcctShape shape) { EnsureShape(shape); CheckInitialized(() => NativeMethods.occt_fit_object(_handle, shape.Id)); }
    public void WindowFit(int x1, int y1, int x2, int y2) => CheckInitialized(() => NativeMethods.occt_window_fit(_handle, x1, y1, x2, y2));

    public void SetView(OcctViewOrientation orientation)
    {
        if (!Enum.IsDefined(orientation)) throw new ArgumentOutOfRangeException(nameof(orientation));
        CheckInitialized(() => NativeMethods.occt_set_view(_handle, (int)orientation));
    }

    public void SetProjection(OcctProjectionType projection)
    {
        if (!Enum.IsDefined(projection)) throw new ArgumentOutOfRangeException(nameof(projection));
        CheckInitialized(() => NativeMethods.occt_set_projection(_handle, (int)projection));
    }

    public void SetPerspectiveFieldOfView(double degrees)
    {
        if (!double.IsFinite(degrees) || degrees <= 0 || degrees >= 180)
            throw new ArgumentOutOfRangeException(nameof(degrees), degrees, "Perspective field of view must be between 0 and 180 degrees.");
        CheckInitialized(() => NativeMethods.occt_set_perspective_fov(_handle, degrees));
    }

    public void SetBackground(Color color) => CheckInitialized(() => NativeMethods.occt_set_background(_handle, color.R / 255.0, color.G / 255.0, color.B / 255.0));

    public void SetDisplayMode(OcctDisplayMode displayMode)
    {
        if (!Enum.IsDefined(displayMode)) throw new ArgumentOutOfRangeException(nameof(displayMode));
        CheckInitialized(() => NativeMethods.occt_set_display_mode(_handle, (int)displayMode));
    }

    public void SetTriedronVisible(bool visible) => CheckInitialized(() => NativeMethods.occt_set_triedron_visible(_handle, visible ? 1 : 0));
    public void SetViewCubeVisible(bool visible) => CheckInitialized(() => NativeMethods.occt_set_view_cube_visible(_handle, visible ? 1 : 0));
    public void SetComputedHlr(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_computed_mode(_handle, enabled ? 1 : 0));

    public void SetDisplayPrecision(double deviationCoefficient, double deviationAngleDegrees, bool applyExisting = true)
    {
        OcctGuard.Positive(deviationCoefficient, nameof(deviationCoefficient));
        OcctGuard.Positive(deviationAngleDegrees, nameof(deviationAngleDegrees));
        CheckInitialized(() => NativeMethods.occt_set_display_precision(_handle, deviationCoefficient, deviationAngleDegrees, applyExisting ? 1 : 0));
    }

    public void SetDefaultMaterial(OcctMaterial material, bool applyExisting = false)
    {
        if (!Enum.IsDefined(material)) throw new ArgumentOutOfRangeException(nameof(material));
        CheckInitialized(() => NativeMethods.occt_set_default_material(_handle, (int)material, applyExisting ? 1 : 0));
    }

    public void SetSceneLighting(double ambientIntensity, double directionalIntensity, OcctVector3d direction, bool headlight = true)
    {
        OcctGuard.NonNegative(ambientIntensity, nameof(ambientIntensity));
        OcctGuard.NonNegative(directionalIntensity, nameof(directionalIntensity));
        OcctGuard.NonZero(direction, nameof(direction));
        CheckInitialized(() => NativeMethods.occt_set_scene_lighting(_handle, ambientIntensity, directionalIntensity, direction, headlight ? 1 : 0));
    }

    public void ResetSceneLighting() => CheckInitialized(() => NativeMethods.occt_reset_scene_lighting(_handle));

    public void SetSelectionTolerance(int pixelTolerance)
    {
        if (pixelTolerance < 0) throw new ArgumentOutOfRangeException(nameof(pixelTolerance));
        CheckInitialized(() => NativeMethods.occt_set_selection_tolerance(_handle, pixelTolerance));
    }

    public void DumpView(string filePath) { ValidatePath(filePath); CheckInitialized(() => NativeMethods.occt_dump_view(_handle, Path.GetFullPath(filePath))); }

    public OcctPoint3d ScreenToWorld(int x, int y)
    {
        EnsureInitialized();
        Check(NativeMethods.occt_screen_to_world(_handle, x, y, out var point));
        return point;
    }

    public Point WorldToScreen(OcctPoint3d point)
    {
        EnsureInitialized();
        Check(NativeMethods.occt_world_to_screen(_handle, point, out var x, out var y));
        return new Point(x, y);
    }

    public void MoveTo(int x, int y) => CheckInitialized(() => NativeMethods.occt_move_to(_handle, x, y));
    public void Select(int x, int y, bool appendSelection = false) => CheckInitialized(() => NativeMethods.occt_select(_handle, x, y, appendSelection ? 1 : 0));
    public void SelectRectangle(int x1, int y1, int x2, int y2, bool appendSelection = false, bool allowOverlap = false) => CheckInitialized(() => NativeMethods.occt_select_rectangle_ex(_handle, x1, y1, x2, y2, appendSelection ? 1 : 0, allowOverlap ? 1 : 0));

    public void SelectObject(OcctObject value, bool appendSelection = false)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_select_object(_handle, value.Id, appendSelection ? 1 : 0));
    }

    public void SetSelectionMode(OcctSelectionMode mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        CheckInitialized(() => NativeMethods.occt_set_selection_mode(_handle, (int)mode));
    }

    public IReadOnlyList<OcctObject> SelectedObjects
    {
        get
        {
            EnsureInitialized();
            var count = NativeMethods.occt_selected_count(_handle);
            var result = new List<OcctObject>(Math.Max(count, 0));
            for (var index = 0; index < count; index++)
            {
                var id = NativeMethods.occt_selected_at(_handle, index);
                if (id > 0) result.Add(new OcctObject(id, GetObjectKind(id), _ownerId));
            }
            return result;
        }
    }

    public OcctShape? FirstSelected
    {
        get
        {
            EnsureInitialized();
            var id = NativeMethods.occt_first_selected(_handle);
            return id > 0 && GetObjectKind(id) == OcctObjectKind.Shape ? new OcctShape(id, _ownerId) : null;
        }
    }

    public OcctObject? FirstSelectedObject
    {
        get
        {
            EnsureInitialized();
            var id = NativeMethods.occt_first_selected(_handle);
            return id > 0 ? new OcctObject(id, GetObjectKind(id), _ownerId) : null;
        }
    }

    public void ClearSelection() => CheckInitialized(() => NativeMethods.occt_clear_selection(_handle));
    public void StartRotation(int x, int y) => CheckInitialized(() => NativeMethods.occt_start_rotation(_handle, x, y));
    public void Rotation(int x, int y) => CheckInitialized(() => NativeMethods.occt_rotation(_handle, x, y));
    public void Pan(int deltaX, int deltaY) => CheckInitialized(() => NativeMethods.occt_pan(_handle, deltaX, deltaY));

    public void Zoom(double factor)
    {
        OcctGuard.Positive(factor, nameof(factor));
        CheckInitialized(() => NativeMethods.occt_zoom(_handle, factor));
    }

    public OcctCameraState GetCamera()
    {
        EnsureInitialized();
        Check(NativeMethods.occt_get_camera(_handle, out var result));
        return result;
    }

    public void SetCamera(OcctCameraState state)
    {
        EnsureInitialized();
        OcctGuard.Positive(state.Scale, nameof(state.Scale));
        OcctGuard.NonZero(state.Up, nameof(state.Up));
        OcctGuard.NonZero(state.Direction, nameof(state.Direction));
        Check(NativeMethods.occt_set_camera(_handle, in state));
    }

    public double ViewScale
    {
        get { EnsureInitialized(); return NativeMethods.occt_get_view_scale(_handle); }
        set { OcctGuard.Positive(value, nameof(value)); CheckInitialized(() => NativeMethods.occt_set_view_scale(_handle, value)); }
    }

    public void SetAntialiasing(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_antialiasing(_handle, enabled ? 1 : 0));
    public void SetGradientBackground(Color first, Color second, OcctGradientFillMethod fillMethod = OcctGradientFillMethod.Vertical) => CheckInitialized(() => NativeMethods.occt_set_gradient_background(_handle, first.R / 255.0, first.G / 255.0, first.B / 255.0, second.R / 255.0, second.G / 255.0, second.B / 255.0, (int)fillMethod));

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

    public OcctObjectKind GetObjectKind(long id)
    {
        EnsureNotDisposed();
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        return (OcctObjectKind)NativeMethods.occt_object_kind(_handle, id);
    }

    public string GetName(IOcctObject value) { EnsureObject(value); return Marshal.PtrToStringUTF8(NativeMethods.occt_get_object_name(_handle, value.Id)) ?? string.Empty; }
    public void SetName(IOcctObject value, string name) { EnsureObject(value); CheckInitialized(() => NativeMethods.occt_set_object_name(_handle, value.Id, name ?? string.Empty)); }
    public void SetColor(IOcctObject value, Color color) { EnsureObject(value); CheckInitialized(() => NativeMethods.occt_set_object_color(_handle, value.Id, color.R / 255.0, color.G / 255.0, color.B / 255.0)); }

    public void SetTransparency(IOcctObject value, double transparency)
    {
        EnsureObject(value);
        OcctGuard.UnitInterval(transparency, nameof(transparency));
        CheckInitialized(() => NativeMethods.occt_set_object_transparency(_handle, value.Id, transparency));
    }

    public void SetVisible(IOcctObject value, bool visible) { EnsureObject(value); CheckInitialized(() => NativeMethods.occt_set_object_visible(_handle, value.Id, visible ? 1 : 0)); }
    public void SetDisplayMode(IOcctObject value, OcctDisplayMode displayMode) { EnsureObject(value); if (!Enum.IsDefined(displayMode)) throw new ArgumentOutOfRangeException(nameof(displayMode)); CheckInitialized(() => NativeMethods.occt_set_object_display_mode(_handle, value.Id, (int)displayMode)); }

    public void SetLineWidth(IOcctObject value, double width)
    {
        EnsureObject(value);
        OcctGuard.Positive(width, nameof(width));
        CheckInitialized(() => NativeMethods.occt_set_object_line_width(_handle, value.Id, width));
    }

    public void SetMaterial(IOcctObject value, OcctMaterial material) { EnsureObject(value); if (!Enum.IsDefined(material)) throw new ArgumentOutOfRangeException(nameof(material)); CheckInitialized(() => NativeMethods.occt_set_object_material(_handle, value.Id, (int)material)); }

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
    public void Redisplay(IOcctObject value) { EnsureObject(value); CheckInitialized(() => NativeMethods.occt_redisplay_object(_handle, value.Id)); }
    public void Highlight(IOcctObject value) { EnsureObject(value); CheckInitialized(() => NativeMethods.occt_highlight_object(_handle, value.Id)); }
    public void Unhighlight(IOcctObject value) { EnsureObject(value); CheckInitialized(() => NativeMethods.occt_unhighlight_object(_handle, value.Id)); }

    public OcctShape CopySelectedSubshape() => CopySelectedSubshape(0);

    public OcctShape CopySelectedSubshape(int index)
    {
        EnsureInitialized();
        OcctGuard.PositiveIndex(index, nameof(index));
        return CheckShape(NativeMethods.occt_copy_selected_subshape_at(_handle, index));
    }

    public OcctShapeType GetShapeType(OcctShape shape) { EnsureShape(shape); return (OcctShapeType)NativeMethods.occt_shape_type(_handle, shape.Id); }
    public bool IsValid(OcctShape shape) { if (!Exists(shape)) return false; return NativeMethods.occt_shape_is_valid(_handle, shape.Id) != 0; }

    public OcctBounds GetBounds(OcctShape shape)
    {
        EnsureShape(shape);
        EnsureInitialized();
        Check(NativeMethods.occt_shape_bounds(_handle, shape.Id, out var result));
        return result;
    }

    public OcctMassProperties GetLinearProperties(OcctShape shape) => GetProperties(shape, NativeMethods.occt_shape_linear_properties);
    public OcctMassProperties GetSurfaceProperties(OcctShape shape) => GetProperties(shape, NativeMethods.occt_shape_surface_properties);
    public OcctMassProperties GetVolumeProperties(OcctShape shape) => GetProperties(shape, NativeMethods.occt_shape_volume_properties);

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

    public IReadOnlyList<OcctShape> GetSubshapes(OcctShape shape, OcctShapeType type) => Enumerable.Range(0, GetTopologyCount(shape, type)).Select(index => GetSubshape(shape, type, index)).ToArray();
    public OcctShape Copy(OcctShape shape, bool hideInput = false) { EnsureShape(shape); EnsureInitialized(); return CheckShape(NativeMethods.occt_copy_shape(_handle, shape.Id, hideInput ? 1 : 0)); }
    public OcctShape Translate(OcctShape shape, OcctVector3d vector, bool hideInput = false) { EnsureShape(shape); EnsureInitialized(); return CheckShape(NativeMethods.occt_translate(_handle, shape.Id, vector, hideInput ? 1 : 0)); }

    public OcctShape Rotate(OcctShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.NonZero(axisDirection, nameof(axisDirection));
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_rotate(_handle, shape.Id, axisPoint, axisDirection, angleDegrees, hideInput ? 1 : 0));
    }

    public OcctShape Scale(OcctShape shape, OcctPoint3d center, double factor, bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.Positive(factor, nameof(factor));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_scale(_handle, shape.Id, center, factor, hideInput ? 1 : 0));
    }

    public OcctShape MirrorPlane(OcctShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal, bool hideInput = false)
    {
        EnsureShape(shape);
        OcctGuard.NonZero(planeNormal, nameof(planeNormal));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_mirror_plane(_handle, shape.Id, planePoint, planeNormal, hideInput ? 1 : 0));
    }

    public long GetShapeHash(OcctShape shape) { EnsureShape(shape); return NativeMethods.occt_shape_hash(_handle, shape.Id); }
    public OcctPoint3d GetVertexPoint(OcctShape vertex) { EnsureShape(vertex); EnsureInitialized(); Check(NativeMethods.occt_vertex_point(_handle, vertex.Id, out var result)); return result; }
    public (OcctPoint3d Start, OcctPoint3d End) GetEdgeEndpoints(OcctShape edge) { EnsureShape(edge); EnsureInitialized(); Check(NativeMethods.occt_edge_endpoints(_handle, edge.Id, out var start, out var end)); return (start, end); }

    public OcctEdgeEvaluation EvaluateEdge(OcctShape edge, double normalizedParameter)
    {
        EnsureShape(edge);
        OcctGuard.UnitInterval(normalizedParameter, nameof(normalizedParameter));
        EnsureInitialized();
        Check(NativeMethods.occt_edge_point_at(_handle, edge.Id, normalizedParameter, out var point, out var tangent));
        return new(point, tangent);
    }

    public OcctCurveType GetCurveType(OcctShape edge) { EnsureShape(edge); return (OcctCurveType)NativeMethods.occt_edge_curve_type(_handle, edge.Id); }
    public OcctSurfaceType GetSurfaceType(OcctShape face) { EnsureShape(face); return (OcctSurfaceType)NativeMethods.occt_face_surface_type(_handle, face.Id); }
    public OcctUvBounds GetUvBounds(OcctShape face) { EnsureShape(face); EnsureInitialized(); Check(NativeMethods.occt_face_uv_bounds(_handle, face.Id, out var result)); return result; }
    public OcctFaceEvaluation EvaluateFace(OcctShape face, double u, double v) { EnsureShape(face); OcctGuard.Finite(u, nameof(u)); OcctGuard.Finite(v, nameof(v)); EnsureInitialized(); Check(NativeMethods.occt_face_point_normal(_handle, face.Id, u, v, out var point, out var normal)); return new(point, normal); }

    private delegate int PropertyCall(IntPtr handle, long id, out OcctMassProperties result);

    private OcctMassProperties GetProperties(OcctShape shape, PropertyCall call)
    {
        EnsureShape(shape);
        EnsureInitialized();
        Check(call(_handle, shape.Id, out var result));
        return result;
    }

    private static void ValidatePath(string path) => ArgumentException.ThrowIfNullOrWhiteSpace(path);

    private OcctShape CheckShape(long id, [CallerMemberName] string? operation = null)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctShape(id, _ownerId);
    }

    private OcctText CheckText(long id, [CallerMemberName] string? operation = null)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctText(id, _ownerId);
    }

    private OcctDimension CheckDimension(long id, [CallerMemberName] string? operation = null)
    {
        if (id <= 0) throw CreateException(operation);
        return new OcctDimension(id, _ownerId);
    }

    private void CheckInitialized(Func<int> nativeCall, [CallerMemberName] string? operation = null)
    {
        EnsureInitialized();
        Check(nativeCall(), operation);
    }

    private void Check(int result, [CallerMemberName] string? operation = null)
    {
        if (result == 0) throw CreateException(operation);
    }

    private OcctException CreateException(string? operation = null)
    {
        var pointer = _handle == IntPtr.Zero ? IntPtr.Zero : NativeMethods.occt_last_error(_handle);
        var nativeMessage = pointer == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(pointer);
        var message = string.IsNullOrWhiteSpace(nativeMessage) ? "The native OCCT operation failed." : nativeMessage;
        return new OcctException(message, operation, nativeMessage);
    }

    private void EnsureObject(IOcctObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureNotDisposed();
        if (IsForeignObject(value))
            throw new ArgumentException("Object belongs to a different OcctEngine.", nameof(value));
        if (value.Id <= 0 || NativeMethods.occt_object_exists(_handle, value.Id) == 0)
            throw new ArgumentException("Object does not belong to this OCCT engine.", nameof(value));
    }

    private void EnsureShape(OcctShape shape)
    {
        EnsureNotDisposed();
        if (shape.OwnerId != 0 && shape.OwnerId != _ownerId)
            throw new ArgumentException("Shape belongs to a different OcctEngine.", nameof(shape));
        if (!shape.IsValid || NativeMethods.occt_object_exists(_handle, shape.Id) == 0 || NativeMethods.occt_object_kind(_handle, shape.Id) != (int)OcctObjectKind.Shape)
            throw new ArgumentException("Shape does not belong to this OCCT engine.", nameof(shape));
    }

    private bool IsForeignObject(IOcctObject value)
    {
        var ownerId = GetOwnerId(value);
        return ownerId != 0 && ownerId != _ownerId;
    }

    private static long GetOwnerId(IOcctObject value) => value switch
    {
        OcctObject item => item.OwnerId,
        OcctShape item => item.OwnerId,
        OcctText item => item.OwnerId,
        OcctDimension item => item.OwnerId,
        _ => 0
    };

    private void EnsureInitialized()
    {
        EnsureNotDisposed();
        if (!Volatile.Read(ref _initialized)) throw new InvalidOperationException("Initialize the OCCT engine with a valid window handle first.");
    }

    private void EnsureNotDisposed() =>
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _handle) == IntPtr.Zero, this);

    public void Dispose()
    {
        ReleaseHandle(throwOnError: true);
        GC.SuppressFinalize(this);
    }

    private void ReleaseHandle(bool throwOnError)
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        Volatile.Write(ref _initialized, false);
        if (handle == IntPtr.Zero) return;

        if (throwOnError)
        {
            NativeMethods.occt_destroy(handle);
            return;
        }

        try
        {
            NativeMethods.occt_destroy(handle);
        }
        catch
        {
            // Finalizers must not allow native unload failures to terminate the process.
        }
    }

    ~OcctEngine() => ReleaseHandle(throwOnError: false);
}
