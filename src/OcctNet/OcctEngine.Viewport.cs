using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void Fit(IEnumerable<OcctShape> shapes, double margin = 0.05)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        if (!double.IsFinite(margin) || margin < 0.0 || margin >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(margin), margin, "Fit margin must be in the range [0, 1).");

        var ids = shapes.Select(shape => shape.Id).Where(id => id > 0).Distinct().ToArray();
        if (ids.Length == 0) throw new ArgumentException("At least one valid shape is required.", nameof(shapes));

        var buffer = Marshal.AllocHGlobal(sizeof(long) * ids.Length);
        try
        {
            Marshal.Copy(ids, 0, buffer, ids.Length);
            EnsureInitialized();
            CheckViewportStatus(ViewportNativeMethods.occt_engine_viewport_fit_objects(
                _handle,
                buffer,
                ids.Length,
                margin));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void FitSelected(double margin = 0.05)
    {
        if (!double.IsFinite(margin) || margin < 0.0 || margin >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(margin), margin, "Fit margin must be in the range [0, 1).");
        EnsureInitialized();
        CheckViewportStatus(ViewportStateNativeMethods.occt_engine_viewport_fit_selected(_handle, margin));
    }

    public void SetZUpView(OcctZUpViewOrientation orientation, bool fitAll = true)
    {
        if (!Enum.IsDefined(orientation)) throw new ArgumentOutOfRangeException(nameof(orientation));
        EnsureInitialized();
        CheckViewportStatus(ViewportNativeMethods.occt_engine_viewport_zup_set(
            _handle,
            (int)orientation,
            fitAll ? 1 : 0));
    }

    public OcctViewportState GetViewportState()
    {
        EnsureInitialized();
        CheckViewportStatus(ViewportStateNativeMethods.occt_engine_viewport_state_get(_handle, out var result));
        if (result.ApiVersion != 1 || result.StructSize < (uint)Marshal.SizeOf<ViewportStateNativeMethods.NativeViewportStateResult>())
            throw new OcctException("Native viewport state ABI is incompatible with this SDK.");
        return result.State;
    }

    public void ResetView() => ResetViewport(NativeViewportResetMask.All);

    public void ResetViewOrientation() => ResetViewport(NativeViewportResetMask.Orientation);

    public void ResetViewMapping() => ResetViewport(NativeViewportResetMask.Mapping);

    public OcctPoint3d GetSceneGravityPoint()
    {
        EnsureInitialized();
        CheckViewportStatus(ViewportStateNativeMethods.occt_engine_viewport_gravity_point_get(_handle, out var result));
        return result;
    }

    public OcctProjectionRay ScreenToRay(int x, int y)
    {
        EnsureInitialized();
        CheckViewportStatus(ViewportNativeMethods.occt_engine_viewport_screen_to_ray(_handle, x, y, out var result));
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

    public void ZoomAtPoint(int x, int y, double delta)
    {
        if (!double.IsFinite(delta) || Math.Abs(delta) <= 1.0e-12)
            throw new ArgumentOutOfRangeException(nameof(delta), delta, "Zoom delta must be finite and non-zero.");
        EnsureInitialized();
        CheckViewportStatus(ViewportNativeMethods.occt_engine_viewport_zoom_at_point(_handle, x, y, delta));
    }

    public void SelectAllVisible()
    {
        EnsureInitialized();
        CheckViewportStatus(ViewportNativeMethods.occt_engine_selection_all_visible(_handle));
    }

    public void InvertSelection()
    {
        EnsureInitialized();
        CheckViewportStatus(ViewportNativeMethods.occt_engine_selection_invert(_handle));
    }

    public void HideSelected()
    {
        EnsureInitialized();
        CheckViewportStatus(ViewportNativeMethods.occt_engine_selection_hide_selected(_handle));
    }

    public void SetAutomaticHighlight(bool enabled)
    {
        EnsureInitialized();
        CheckViewportStatus(ViewportNativeMethods.occt_engine_selection_automatic_highlight_set(
            _handle,
            enabled ? 1 : 0));
    }

    public void SetMsaaSamples(int samples)
    {
        if (samples < 0 || samples > 16) throw new ArgumentOutOfRangeException(nameof(samples));
        UpdateRendering(RenderingOptions(NativeViewportRenderingUpdateMask.MsaaSamples, msaaSamples: samples));
    }

    public void SetRenderResolutionScale(double scale)
    {
        if (!double.IsFinite(scale) || scale < 0.25 || scale > 4.0)
            throw new ArgumentOutOfRangeException(nameof(scale));
        UpdateRendering(RenderingOptions(NativeViewportRenderingUpdateMask.ResolutionScale, resolutionScale: scale));
    }

    public void SetRenderResolution(double dpi)
    {
        if (!double.IsFinite(dpi) || dpi < 36.0 || dpi > 600.0)
            throw new ArgumentOutOfRangeException(nameof(dpi));
        UpdateRendering(RenderingOptions(NativeViewportRenderingUpdateMask.ResolutionDpi, resolutionDpi: dpi));
    }

    public void SetRenderingMethod(OcctRenderingMethod method)
    {
        if (!Enum.IsDefined(method)) throw new ArgumentOutOfRangeException(nameof(method));
        UpdateRendering(RenderingOptions(NativeViewportRenderingUpdateMask.Method, renderingMethod: (int)method));
    }

    public void SetShadowsEnabled(bool enabled) =>
        UpdateRendering(RenderingOptions(NativeViewportRenderingUpdateMask.Shadows, shadowsEnabled: enabled ? 1 : 0));

    public void SetImmediateUpdate(bool enabled) =>
        UpdateRendering(RenderingOptions(NativeViewportRenderingUpdateMask.ImmediateUpdate, immediateUpdate: enabled ? 1 : 0));

    public void SetFrustumCulling(bool enabled) =>
        UpdateRendering(RenderingOptions(NativeViewportRenderingUpdateMask.FrustumCulling, frustumCullingEnabled: enabled ? 1 : 0));

    public void SetFaceBoundariesVisible(bool visible, bool applyExisting = true) =>
        UpdateRendering(RenderingOptions(
            NativeViewportRenderingUpdateMask.FaceBoundaries,
            faceBoundariesVisible: visible ? 1 : 0,
            applyFaceBoundariesToExisting: applyExisting ? 1 : 0));

    private void ResetViewport(NativeViewportResetMask resetMask)
    {
        EnsureInitialized();
        CheckViewportStatus(ViewportStateNativeMethods.occt_engine_viewport_reset(_handle, resetMask));
    }

    private void UpdateRendering(ViewportNativeMethods.NativeViewportRenderingOptions options)
    {
        EnsureInitialized();
        CheckViewportStatus(ViewportNativeMethods.occt_engine_viewport_rendering_update(_handle, in options));
    }

    private static ViewportNativeMethods.NativeViewportRenderingOptions RenderingOptions(
        NativeViewportRenderingUpdateMask updateMask,
        int msaaSamples = 0,
        double resolutionScale = 1.0,
        double resolutionDpi = 96.0,
        int renderingMethod = 0,
        int shadowsEnabled = 0,
        int immediateUpdate = 0,
        int frustumCullingEnabled = 0,
        int faceBoundariesVisible = 0,
        int applyFaceBoundariesToExisting = 0) => new()
    {
        StructSize = (uint)Marshal.SizeOf<ViewportNativeMethods.NativeViewportRenderingOptions>(),
        ApiVersion = 1,
        UpdateMask = updateMask,
        MsaaSamples = msaaSamples,
        ResolutionScale = resolutionScale,
        ResolutionDpi = resolutionDpi,
        RenderingMethod = renderingMethod,
        ShadowsEnabled = shadowsEnabled,
        ImmediateUpdate = immediateUpdate,
        FrustumCullingEnabled = frustumCullingEnabled,
        FaceBoundariesVisible = faceBoundariesVisible,
        ApplyFaceBoundariesToExisting = applyFaceBoundariesToExisting
    };

    private void CheckViewportStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
