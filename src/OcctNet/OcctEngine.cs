using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine : IDisposable
{
    private IntPtr _handle;
    private bool _initialized;

    public OcctEngine()
    {
        OcctRuntime.Configure();
        _handle = NativeMethods.occt_create();
        if (_handle == IntPtr.Zero) throw new OcctException("Unable to create the native OCCT engine.");
    }

    public bool IsInitialized => _initialized;
    public static string OcctVersion => Marshal.PtrToStringUTF8(NativeMethods.occt_version()) ?? "Unknown";
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
                .Select(id => new OcctObject(id, GetObjectKind(id)))
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
                .Select(id => new OcctShape(id))
                .ToArray();
        }
    }

    public void Initialize(IntPtr windowHandle)
    {
        EnsureNotDisposed();
        if (_initialized) return;
        Check(NativeMethods.occt_initialize(_handle, windowHandle));
        _initialized = true;
    }

    public void Resize() => CheckInitialized(() => NativeMethods.occt_resize(_handle));
    public void Redraw() => CheckInitialized(() => NativeMethods.occt_redraw(_handle));
    public void FitAll() => CheckInitialized(() => NativeMethods.occt_fit_all(_handle));
    public void Fit(OcctShape shape) => CheckInitialized(() => NativeMethods.occt_fit_object(_handle, shape.Id));
    public void WindowFit(int x1, int y1, int x2, int y2) => CheckInitialized(() => NativeMethods.occt_window_fit(_handle, x1, y1, x2, y2));
    public void SetView(OcctViewOrientation orientation) => CheckInitialized(() => NativeMethods.occt_set_view(_handle, (int)orientation));
    public void SetProjection(OcctProjectionType projection) => CheckInitialized(() => NativeMethods.occt_set_projection(_handle, (int)projection));
    public void SetPerspectiveFieldOfView(double degrees) => CheckInitialized(() => NativeMethods.occt_set_perspective_fov(_handle, degrees));
    public void SetBackground(Color color) => CheckInitialized(() => NativeMethods.occt_set_background(_handle, color.R / 255.0, color.G / 255.0, color.B / 255.0));
    public void SetDisplayMode(OcctDisplayMode displayMode) => CheckInitialized(() => NativeMethods.occt_set_display_mode(_handle, (int)displayMode));
    public void SetTriedronVisible(bool visible) => CheckInitialized(() => NativeMethods.occt_set_triedron_visible(_handle, visible ? 1 : 0));
    public void SetViewCubeVisible(bool visible) => CheckInitialized(() => NativeMethods.occt_set_view_cube_visible(_handle, visible ? 1 : 0));
    public void SetComputedHlr(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_computed_mode(_handle, enabled ? 1 : 0));
    public void SetDisplayPrecision(double deviationCoefficient, double deviationAngleDegrees, bool applyExisting = true) => CheckInitialized(() => NativeMethods.occt_set_display_precision(_handle, deviationCoefficient, deviationAngleDegrees, applyExisting ? 1 : 0));
    public void SetDefaultMaterial(OcctMaterial material, bool applyExisting = false) => CheckInitialized(() => NativeMethods.occt_set_default_material(_handle, (int)material, applyExisting ? 1 : 0));
    public void SetSceneLighting(double ambientIntensity, double directionalIntensity, OcctVector3d direction, bool headlight = true) => CheckInitialized(() => NativeMethods.occt_set_scene_lighting(_handle, ambientIntensity, directionalIntensity, direction, headlight ? 1 : 0));
    public void ResetSceneLighting() => CheckInitialized(() => NativeMethods.occt_reset_scene_lighting(_handle));
    public void SetSelectionTolerance(int pixelTolerance) => CheckInitialized(() => NativeMethods.occt_set_selection_tolerance(_handle, pixelTolerance));
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
    public void SelectObject(OcctObject value, bool appendSelection = false) => CheckInitialized(() => NativeMethods.occt_select_object(_handle, value.Id, appendSelection ? 1 : 0));
    public void SetSelectionMode(OcctSelectionMode mode) => CheckInitialized(() => NativeMethods.occt_set_selection_mode(_handle, (int)mode));

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
                if (id > 0) result.Add(new OcctObject(id, GetObjectKind(id)));
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
            return id > 0 && GetObjectKind(id) == OcctObjectKind.Shape ? new OcctShape(id) : null;
        }
    }

    public OcctObject? FirstSelectedObject
    {
        get
        {
            EnsureInitialized();
            var id = NativeMethods.occt_first_selected(_handle);
            return id > 0 ? new OcctObject(id, GetObjectKind(id)) : null;
        }
    }

    public void ClearSelection() => CheckInitialized(() => NativeMethods.occt_clear_selection(_handle));
    public void StartRotation(int x, int y) => CheckInitialized(() => NativeMethods.occt_start_rotation(_handle, x, y));
    public void Rotation(int x, int y) => CheckInitialized(() => NativeMethods.occt_rotation(_handle, x, y));
    public void Pan(int deltaX, int deltaY) => CheckInitialized(() => NativeMethods.occt_pan(_handle, deltaX, deltaY));
    public void Zoom(double factor) => CheckInitialized(() => NativeMethods.occt_zoom(_handle, factor));

    public OcctCameraState GetCamera()
    {
        EnsureInitialized();
        Check(NativeMethods.occt_get_camera(_handle, out var result));
        return result;
    }

    public void SetCamera(OcctCameraState state)
    {
        EnsureInitialized();
        Check(NativeMethods.occt_set_camera(_handle, in state));
    }
    public double ViewScale { get { EnsureInitialized(); return NativeMethods.occt_get_view_scale(_handle); } set => CheckInitialized(() => NativeMethods.occt_set_view_scale(_handle, value)); }
    public void SetAntialiasing(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_antialiasing(_handle, enabled ? 1 : 0));
    public void SetGradientBackground(Color first, Color second, OcctGradientFillMethod fillMethod = OcctGradientFillMethod.Vertical) => CheckInitialized(() => NativeMethods.occt_set_gradient_background(_handle, first.R / 255.0, first.G / 255.0, first.B / 255.0, second.R / 255.0, second.G / 255.0, second.B / 255.0, (int)fillMethod));

    public bool Exists(IOcctObject value) { EnsureNotDisposed(); return NativeMethods.occt_object_exists(_handle, value.Id) != 0; }
    public OcctObjectKind GetObjectKind(long id) { EnsureNotDisposed(); return (OcctObjectKind)NativeMethods.occt_object_kind(_handle, id); }
    public string GetName(IOcctObject value) { EnsureNotDisposed(); return Marshal.PtrToStringUTF8(NativeMethods.occt_get_object_name(_handle, value.Id)) ?? string.Empty; }
    public void SetName(IOcctObject value, string name) => CheckInitialized(() => NativeMethods.occt_set_object_name(_handle, value.Id, name ?? string.Empty));
    public void SetColor(IOcctObject value, Color color) => CheckInitialized(() => NativeMethods.occt_set_object_color(_handle, value.Id, color.R / 255.0, color.G / 255.0, color.B / 255.0));
    public void SetTransparency(IOcctObject value, double transparency) => CheckInitialized(() => NativeMethods.occt_set_object_transparency(_handle, value.Id, transparency));
    public void SetVisible(IOcctObject value, bool visible) => CheckInitialized(() => NativeMethods.occt_set_object_visible(_handle, value.Id, visible ? 1 : 0));
    public void SetDisplayMode(IOcctObject value, OcctDisplayMode displayMode) => CheckInitialized(() => NativeMethods.occt_set_object_display_mode(_handle, value.Id, (int)displayMode));
    public void SetLineWidth(IOcctObject value, double width) => CheckInitialized(() => NativeMethods.occt_set_object_line_width(_handle, value.Id, width));
    public void SetMaterial(IOcctObject value, OcctMaterial material) => CheckInitialized(() => NativeMethods.occt_set_object_material(_handle, value.Id, (int)material));
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
            if (value.Id <= 0) throw new ArgumentException("Object IDs must be greater than zero.", nameof(values));
            ids.Add(value.Id);
        }

        if (ids.Count == 0) return;
        var objectIds = ids.ToArray();
        Check(NativeMethods.occt_delete_objects(_handle, objectIds, objectIds.Length));
    }
    public void Clear() => CheckInitialized(() => NativeMethods.occt_clear(_handle));
    public void ShowAll() => CheckInitialized(() => NativeMethods.occt_show_all(_handle));
    public void HideAll() => CheckInitialized(() => NativeMethods.occt_hide_all(_handle));
    public void Redisplay(IOcctObject value) => CheckInitialized(() => NativeMethods.occt_redisplay_object(_handle, value.Id));
    public void Highlight(IOcctObject value) => CheckInitialized(() => NativeMethods.occt_highlight_object(_handle, value.Id));
    public void Unhighlight(IOcctObject value) => CheckInitialized(() => NativeMethods.occt_unhighlight_object(_handle, value.Id));
    public OcctShape CopySelectedSubshape() => CopySelectedSubshape(0);
    public OcctShape CopySelectedSubshape(int index)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_copy_selected_subshape_at(_handle, index));
    }

    public OcctShapeType GetShapeType(OcctShape shape) { EnsureNotDisposed(); return (OcctShapeType)NativeMethods.occt_shape_type(_handle, shape.Id); }
    public bool IsValid(OcctShape shape) { EnsureNotDisposed(); return NativeMethods.occt_shape_is_valid(_handle, shape.Id) != 0; }

    public OcctBounds GetBounds(OcctShape shape)
    {
        EnsureInitialized();
        Check(NativeMethods.occt_shape_bounds(_handle, shape.Id, out var result));
        return result;
    }

    public OcctMassProperties GetLinearProperties(OcctShape shape) => GetProperties(shape, NativeMethods.occt_shape_linear_properties);
    public OcctMassProperties GetSurfaceProperties(OcctShape shape) => GetProperties(shape, NativeMethods.occt_shape_surface_properties);
    public OcctMassProperties GetVolumeProperties(OcctShape shape) => GetProperties(shape, NativeMethods.occt_shape_volume_properties);

    public OcctDistanceResult Distance(OcctShape first, OcctShape second)
    {
        EnsureInitialized();
        Check(NativeMethods.occt_shape_distance(_handle, first.Id, second.Id, out var result));
        return result;
    }

    public int GetTopologyCount(OcctShape shape, OcctShapeType type) { EnsureNotDisposed(); return NativeMethods.occt_topology_count(_handle, shape.Id, (int)type); }
    public OcctShape GetSubshape(OcctShape shape, OcctShapeType type, int index) { EnsureInitialized(); return CheckShape(NativeMethods.occt_get_subshape(_handle, shape.Id, (int)type, index)); }
    public IReadOnlyList<OcctShape> GetSubshapes(OcctShape shape, OcctShapeType type) => Enumerable.Range(0, GetTopologyCount(shape, type)).Select(index => GetSubshape(shape, type, index)).ToArray();
    public OcctShape Copy(OcctShape shape, bool hideInput = false) { EnsureInitialized(); return CheckShape(NativeMethods.occt_copy_shape(_handle, shape.Id, hideInput ? 1 : 0)); }
    public OcctShape Translate(OcctShape shape, OcctVector3d vector, bool hideInput = false) { EnsureInitialized(); return CheckShape(NativeMethods.occt_translate(_handle, shape.Id, vector, hideInput ? 1 : 0)); }
    public OcctShape Rotate(OcctShape shape, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees, bool hideInput = false) { EnsureInitialized(); return CheckShape(NativeMethods.occt_rotate(_handle, shape.Id, axisPoint, axisDirection, angleDegrees, hideInput ? 1 : 0)); }
    public OcctShape Scale(OcctShape shape, OcctPoint3d center, double factor, bool hideInput = false) { EnsureInitialized(); return CheckShape(NativeMethods.occt_scale(_handle, shape.Id, center, factor, hideInput ? 1 : 0)); }
    public OcctShape MirrorPlane(OcctShape shape, OcctPoint3d planePoint, OcctVector3d planeNormal, bool hideInput = false) { EnsureInitialized(); return CheckShape(NativeMethods.occt_mirror_plane(_handle, shape.Id, planePoint, planeNormal, hideInput ? 1 : 0)); }

    public long GetShapeHash(OcctShape shape) { EnsureNotDisposed(); return NativeMethods.occt_shape_hash(_handle, shape.Id); }
    public OcctPoint3d GetVertexPoint(OcctShape vertex) { EnsureInitialized(); Check(NativeMethods.occt_vertex_point(_handle, vertex.Id, out var result)); return result; }
    public (OcctPoint3d Start, OcctPoint3d End) GetEdgeEndpoints(OcctShape edge) { EnsureInitialized(); Check(NativeMethods.occt_edge_endpoints(_handle, edge.Id, out var start, out var end)); return (start, end); }
    public OcctEdgeEvaluation EvaluateEdge(OcctShape edge, double normalizedParameter) { EnsureInitialized(); Check(NativeMethods.occt_edge_point_at(_handle, edge.Id, normalizedParameter, out var point, out var tangent)); return new(point, tangent); }
    public OcctCurveType GetCurveType(OcctShape edge) { EnsureNotDisposed(); return (OcctCurveType)NativeMethods.occt_edge_curve_type(_handle, edge.Id); }
    public OcctSurfaceType GetSurfaceType(OcctShape face) { EnsureNotDisposed(); return (OcctSurfaceType)NativeMethods.occt_face_surface_type(_handle, face.Id); }
    public OcctUvBounds GetUvBounds(OcctShape face) { EnsureInitialized(); Check(NativeMethods.occt_face_uv_bounds(_handle, face.Id, out var result)); return result; }
    public OcctFaceEvaluation EvaluateFace(OcctShape face, double u, double v) { EnsureInitialized(); Check(NativeMethods.occt_face_point_normal(_handle, face.Id, u, v, out var point, out var normal)); return new(point, normal); }

    private delegate int PropertyCall(IntPtr handle, long id, out OcctMassProperties result);
    private OcctMassProperties GetProperties(OcctShape shape, PropertyCall call)
    {
        EnsureInitialized();
        Check(call(_handle, shape.Id, out var result));
        return result;
    }

    private static void ValidatePath(string path) => ArgumentException.ThrowIfNullOrWhiteSpace(path);
    private OcctShape CheckShape(long id) { if (id <= 0) throw CreateException(); return new OcctShape(id); }
    private OcctText CheckText(long id) { if (id <= 0) throw CreateException(); return new OcctText(id); }
    private OcctDimension CheckDimension(long id) { if (id <= 0) throw CreateException(); return new OcctDimension(id); }
    private void CheckInitialized(Func<int> nativeCall) { EnsureInitialized(); Check(nativeCall()); }
    private void Check(int result) { if (result == 0) throw CreateException(); }

    private OcctException CreateException()
    {
        var pointer = NativeMethods.occt_last_error(_handle);
        var message = pointer == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(pointer);
        return new OcctException(string.IsNullOrWhiteSpace(message) ? "The native OCCT operation failed." : message);
    }

    private void EnsureInitialized()
    {
        EnsureNotDisposed();
        if (!_initialized) throw new InvalidOperationException("Initialize the OCCT engine with a valid window handle first.");
    }

    private void EnsureNotDisposed() => ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

    public void Dispose()
    {
        if (_handle == IntPtr.Zero) return;
        NativeMethods.occt_destroy(_handle);
        _handle = IntPtr.Zero;
        _initialized = false;
        GC.SuppressFinalize(this);
    }

    ~OcctEngine() => Dispose();
}
