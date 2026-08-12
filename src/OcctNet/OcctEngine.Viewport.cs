namespace OcctNet;

public sealed partial class OcctEngine
{
    public void Fit(IEnumerable<OcctShape> shapes, double margin = 0.05)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        var ids = shapes.Select(shape => shape.Id).Where(id => id > 0).Distinct().ToArray();
        if (ids.Length == 0) throw new ArgumentException("At least one valid shape is required.", nameof(shapes));
        CheckInitialized(() => NativeMethods.occt_fit_objects(_handle, ids, ids.Length, margin));
    }

    public void FitSelected(double margin = 0.05) =>
        CheckInitialized(() => NativeMethods.occt_fit_selected(_handle, margin));

    public void SetZUpView(OcctZUpViewOrientation orientation, bool fitAll = true) =>
        CheckInitialized(() => NativeMethods.occt_set_zup_view(_handle, (int)orientation, fitAll ? 1 : 0));

    public OcctViewportState GetViewportState()
    {
        EnsureInitialized();
        Check(NativeMethods.occt_get_viewport_state(_handle, out var result));
        return result;
    }

    public void ResetView() => CheckInitialized(() => NativeMethods.occt_reset_view(_handle));

    public void ResetViewOrientation() =>
        CheckInitialized(() => NativeMethods.occt_reset_view_orientation(_handle));

    public void ResetViewMapping() =>
        CheckInitialized(() => NativeMethods.occt_reset_view_mapping(_handle));

    public OcctPoint3d GetSceneGravityPoint()
    {
        EnsureInitialized();
        Check(NativeMethods.occt_get_scene_gravity_point(_handle, out var result));
        return result;
    }

    public OcctProjectionRay ScreenToRay(int x, int y)
    {
        EnsureInitialized();
        Check(NativeMethods.occt_screen_to_ray(_handle, x, y, out var result));
        return result;
    }

    public OcctPoint3d ScreenToPlane(
        int x,
        int y,
        OcctPoint3d planePoint,
        OcctVector3d planeNormal)
    {
        if (!TryScreenToPlane(x, y, planePoint, planeNormal, out var result))
            throw new InvalidOperationException("The screen projection ray is parallel to the target plane.");
        return result;
    }

    public bool TryScreenToPlane(
        int x,
        int y,
        OcctPoint3d planePoint,
        OcctVector3d planeNormal,
        out OcctPoint3d result)
    {
        var normalLengthSquared = planeNormal.X * planeNormal.X
                                  + planeNormal.Y * planeNormal.Y
                                  + planeNormal.Z * planeNormal.Z;
        if (!double.IsFinite(normalLengthSquared) || normalLengthSquared <= 1.0e-24)
            throw new ArgumentException("Plane normal must be finite and non-zero.", nameof(planeNormal));

        var ray = ScreenToRay(x, y);
        var denominator = ray.Direction.X * planeNormal.X
                          + ray.Direction.Y * planeNormal.Y
                          + ray.Direction.Z * planeNormal.Z;
        if (!double.IsFinite(denominator) || Math.Abs(denominator) <= 1.0e-12)
        {
            result = default;
            return false;
        }

        var dx = planePoint.X - ray.Origin.X;
        var dy = planePoint.Y - ray.Origin.Y;
        var dz = planePoint.Z - ray.Origin.Z;
        var parameter = (dx * planeNormal.X + dy * planeNormal.Y + dz * planeNormal.Z) / denominator;
        result = new OcctPoint3d(
            ray.Origin.X + ray.Direction.X * parameter,
            ray.Origin.Y + ray.Direction.Y * parameter,
            ray.Origin.Z + ray.Direction.Z * parameter);
        return double.IsFinite(result.X) && double.IsFinite(result.Y) && double.IsFinite(result.Z);
    }

    public void ZoomAtPoint(int x, int y, double delta) =>
        CheckInitialized(() => NativeMethods.occt_zoom_at_point(_handle, x, y, delta));

    public void SelectAllVisible() => CheckInitialized(() => NativeMethods.occt_select_all_visible(_handle));
    public void InvertSelection() => CheckInitialized(() => NativeMethods.occt_invert_selection(_handle));
    public void HideSelected() => CheckInitialized(() => NativeMethods.occt_hide_selected(_handle));
    public void SetAutomaticHighlight(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_automatic_highlight(_handle, enabled ? 1 : 0));

    public void SetMsaaSamples(int samples) => CheckInitialized(() => NativeMethods.occt_set_msaa_samples(_handle, samples));
    public void SetRenderResolutionScale(double scale) => CheckInitialized(() => NativeMethods.occt_set_render_resolution_scale(_handle, scale));
    public void SetRenderResolution(double dpi) => CheckInitialized(() => NativeMethods.occt_set_render_resolution(_handle, dpi));
    public void SetRenderingMethod(OcctRenderingMethod method) => CheckInitialized(() => NativeMethods.occt_set_rendering_method(_handle, (int)method));
    public void SetShadowsEnabled(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_shadows_enabled(_handle, enabled ? 1 : 0));
    public void SetImmediateUpdate(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_immediate_update(_handle, enabled ? 1 : 0));
    public void SetFrustumCulling(bool enabled) => CheckInitialized(() => NativeMethods.occt_set_frustum_culling(_handle, enabled ? 1 : 0));
    public void SetFaceBoundariesVisible(bool visible, bool applyExisting = true) =>
        CheckInitialized(() => NativeMethods.occt_set_face_boundaries_visible(_handle, visible ? 1 : 0, applyExisting ? 1 : 0));
}
