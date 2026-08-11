using System.Drawing;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void Initialize(IntPtr windowHandle)
    {
        EnsureNotDisposed();
        if (windowHandle == IntPtr.Zero) throw new ArgumentException("Window handle must not be zero.", nameof(windowHandle));
        if (Volatile.Read(ref _initialized)) return;
        Check(NativeMethods.occt_initialize(_handle, windowHandle));
        Volatile.Write(ref _initialized, true);
    }

    public void Resize() => CheckInitialized(() => NativeMethods.occt_resize(_handle));

    /// <summary>
    /// Synchronizes the OCCT render surface with the native window size without drawing a frame.
    /// UI adapters can coalesce repeated resize notifications and call <see cref="Redraw"/> once.
    /// </summary>
    public void ResizeSurface() => CheckInitialized(() => NativeMethods.occt_resize_surface(_handle));

    public void Redraw() => CheckInitialized(() => NativeMethods.occt_redraw(_handle));
    public void FitAll() => CheckInitialized(() => NativeMethods.occt_fit_all(_handle));

    public void Fit(OcctShape shape)
    {
        EnsureShape(shape);
        CheckInitialized(() => NativeMethods.occt_fit_object(_handle, shape.Id));
    }

    public void WindowFit(int x1, int y1, int x2, int y2) =>
        CheckInitialized(() => NativeMethods.occt_window_fit(_handle, x1, y1, x2, y2));

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

    public void SetBackground(Color color) =>
        CheckInitialized(() => NativeMethods.occt_set_background(_handle, color.R / 255.0, color.G / 255.0, color.B / 255.0));

    public void SetDisplayMode(OcctDisplayMode displayMode)
    {
        if (!Enum.IsDefined(displayMode)) throw new ArgumentOutOfRangeException(nameof(displayMode));
        CheckInitialized(() => NativeMethods.occt_set_display_mode(_handle, (int)displayMode));
    }

    public void SetTriedronVisible(bool visible) =>
        CheckInitialized(() => NativeMethods.occt_set_triedron_visible(_handle, visible ? 1 : 0));

    public void SetViewCubeVisible(bool visible) =>
        CheckInitialized(() => NativeMethods.occt_set_view_cube_visible(_handle, visible ? 1 : 0));

    public void SetViewCubeLanguage(OcctViewCubeLanguage language)
    {
        if (!Enum.IsDefined(language)) throw new ArgumentOutOfRangeException(nameof(language));
        CheckInitialized(() => NativeMethods.occt_set_view_cube_language(_handle, (int)language));
    }

    public void SetComputedHlr(bool enabled) =>
        CheckInitialized(() => NativeMethods.occt_set_computed_mode(_handle, enabled ? 1 : 0));

    public void SetDisplayPrecision(double deviationCoefficient, double deviationAngleDegrees, bool applyExisting = true)
    {
        OcctGuard.Positive(deviationCoefficient, nameof(deviationCoefficient));
        OcctGuard.Positive(deviationAngleDegrees, nameof(deviationAngleDegrees));
        CheckInitialized(() => NativeMethods.occt_set_display_precision(
            _handle,
            deviationCoefficient,
            deviationAngleDegrees,
            applyExisting ? 1 : 0));
    }

    public void SetDefaultMaterial(OcctMaterial material, bool applyExisting = false)
    {
        if (!Enum.IsDefined(material)) throw new ArgumentOutOfRangeException(nameof(material));
        CheckInitialized(() => NativeMethods.occt_set_default_material(_handle, (int)material, applyExisting ? 1 : 0));
    }

    public void ResetSceneLighting() => CheckInitialized(() => NativeMethods.occt_reset_scene_lighting(_handle));

    public void SetSelectionTolerance(int pixelTolerance)
    {
        if (pixelTolerance < 0) throw new ArgumentOutOfRangeException(nameof(pixelTolerance));
        CheckInitialized(() => NativeMethods.occt_set_selection_tolerance(_handle, pixelTolerance));
    }

    public void DumpView(string filePath)
    {
        ValidatePath(filePath);
        CheckInitialized(() => NativeMethods.occt_dump_view(_handle, Path.GetFullPath(filePath)));
    }

    public OcctPoint3d ScreenToWorld(int x, int y)
    {
        EnsureInitialized();
        Check(NativeMethods.occt_screen_to_world(_handle, x, y, out var point));
        return point;
    }

    public Point WorldToScreen(OcctPoint3d point)
    {
        OcctGuard.Finite(point, nameof(point));
        EnsureInitialized();
        Check(NativeMethods.occt_world_to_screen(_handle, point, out var x, out var y));
        return new Point(x, y);
    }

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
        OcctGuard.Finite(state.Eye, nameof(state.Eye));
        OcctGuard.Finite(state.Center, nameof(state.Center));
        OcctGuard.Positive(state.Scale, nameof(state.Scale));
        OcctGuard.NonZero(state.Up, nameof(state.Up));
        OcctGuard.NonZero(state.Direction, nameof(state.Direction));
        EnsureInitialized();
        Check(NativeMethods.occt_set_camera(_handle, in state));
    }

    public double ViewScale
    {
        get
        {
            EnsureInitialized();
            return NativeMethods.occt_get_view_scale(_handle);
        }
        set
        {
            OcctGuard.Positive(value, nameof(value));
            CheckInitialized(() => NativeMethods.occt_set_view_scale(_handle, value));
        }
    }

    public void SetAntialiasing(bool enabled) =>
        CheckInitialized(() => NativeMethods.occt_set_antialiasing(_handle, enabled ? 1 : 0));

    public void SetGradientBackground(
        Color first,
        Color second,
        OcctGradientFillMethod fillMethod = OcctGradientFillMethod.Vertical)
    {
        if (!Enum.IsDefined(fillMethod)) throw new ArgumentOutOfRangeException(nameof(fillMethod));
        CheckInitialized(() => NativeMethods.occt_set_gradient_background(
            _handle,
            first.R / 255.0,
            first.G / 255.0,
            first.B / 255.0,
            second.R / 255.0,
            second.G / 255.0,
            second.B / 255.0,
            (int)fillMethod));
    }
}
