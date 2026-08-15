using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void Initialize(IntPtr windowHandle) =>
        InitializeNativeSurface(OcctNativeSurfaceKind.Auto, windowHandle);

    public void Resize()
    {
        EnsureInitialized();
        CheckViewStatus(SurfaceNativeMethods.occt_engine_surface_resize(_handle, 1));
    }

    /// <summary>
    /// Synchronizes the OCCT render surface with the native window size without drawing a frame.
    /// UI adapters can coalesce repeated resize notifications and call <see cref="Redraw"/> once.
    /// </summary>
    public void ResizeSurface()
    {
        EnsureInitialized();
        CheckViewStatus(SurfaceNativeMethods.occt_engine_surface_resize(_handle, 0));
    }

    public void Redraw()
    {
        EnsureInitialized();
        CheckViewStatus(SurfaceNativeMethods.occt_engine_surface_redraw(_handle));
    }

    public void FitAll()
    {
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_fit_all(_handle));
    }

    public void Fit(OcctShape shape)
    {
        EnsureShape(shape);
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_fit_object(_handle, shape.Id));
    }

    public void WindowFit(int x1, int y1, int x2, int y2)
    {
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_window_fit(_handle, x1, y1, x2, y2));
    }

    public void SetView(OcctViewOrientation orientation)
    {
        if (!Enum.IsDefined(orientation)) throw new ArgumentOutOfRangeException(nameof(orientation));
        UpdateViewState(ViewStateOptions(
            NativeViewerViewStateUpdateMask.Orientation,
            orientation: (int)orientation,
            fitAfterOrientation: true));
    }

    public void SetProjection(OcctProjectionType projection)
    {
        if (!Enum.IsDefined(projection)) throw new ArgumentOutOfRangeException(nameof(projection));
        UpdateViewState(ViewStateOptions(
            NativeViewerViewStateUpdateMask.Projection,
            projectionType: (int)projection));
    }

    public void SetPerspectiveFieldOfView(double degrees)
    {
        if (!double.IsFinite(degrees) || degrees <= 1.0 || degrees >= 179.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(degrees),
                degrees,
                "Perspective field of view must be between 1 and 179 degrees.");
        }
        UpdateViewState(ViewStateOptions(
            NativeViewerViewStateUpdateMask.PerspectiveFov,
            perspectiveFovDegrees: degrees));
    }

    public void SetBackground(Color color) =>
        UpdateViewState(ViewStateOptions(
            NativeViewerViewStateUpdateMask.SolidBackground,
            backgroundFirst: ToNativeViewColor(color)));

    public void SetDisplayMode(OcctDisplayMode displayMode)
    {
        if (!Enum.IsDefined(displayMode)) throw new ArgumentOutOfRangeException(nameof(displayMode));
        UpdateViewState(ViewStateOptions(
            NativeViewerViewStateUpdateMask.DisplayMode,
            displayMode: (int)displayMode));
    }

    public void SetTriedronVisible(bool visible) =>
        UpdateViewState(ViewStateOptions(
            NativeViewerViewStateUpdateMask.TriedronVisible,
            triedronVisible: visible));

    public void SetViewCubeVisible(bool visible) =>
        UpdateViewState(ViewStateOptions(
            NativeViewerViewStateUpdateMask.ViewCubeVisible,
            viewCubeVisible: visible));

    public void SetViewCubeLanguage(OcctViewCubeLanguage language)
    {
        if (!Enum.IsDefined(language)) throw new ArgumentOutOfRangeException(nameof(language));
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_cube_language_set(_handle, (int)language));
    }

    public void SetComputedHlr(bool enabled) =>
        UpdateViewState(ViewStateOptions(
            NativeViewerViewStateUpdateMask.ComputedMode,
            computedMode: enabled));

    public void SetDisplayPrecision(
        double deviationCoefficient,
        double deviationAngleDegrees,
        bool applyExisting = true)
    {
        OcctGuard.Positive(deviationCoefficient, nameof(deviationCoefficient));
        if (!double.IsFinite(deviationAngleDegrees) ||
            deviationAngleDegrees <= 0.0 || deviationAngleDegrees >= 90.0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviationAngleDegrees));
        }

        UpdateDisplayQuality(new NativeViewerDisplayQualityOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerDisplayQualityOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerDisplayQualityUpdateMask.Precision,
            DeviationCoefficient = deviationCoefficient,
            DeviationAngleDegrees = deviationAngleDegrees,
            ApplyPrecisionToExisting = applyExisting ? 1 : 0
        });
    }

    public void SetDefaultMaterial(OcctMaterial material, bool applyExisting = false)
    {
        if (!Enum.IsDefined(material)) throw new ArgumentOutOfRangeException(nameof(material));
        UpdateDisplayQuality(new NativeViewerDisplayQualityOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerDisplayQualityOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerDisplayQualityUpdateMask.DefaultMaterial,
            Material = (int)material,
            ApplyMaterialToExisting = applyExisting ? 1 : 0
        });
    }

    public void SetSelectionTolerance(int pixelTolerance)
    {
        if (pixelTolerance < 0 || pixelTolerance > 100)
            throw new ArgumentOutOfRangeException(nameof(pixelTolerance));
        UpdateSelectionSettings(new NativeViewerSelectionSettingsOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerSelectionSettingsOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerSelectionSettingsUpdateMask.Tolerance,
            PixelTolerance = pixelTolerance
        });
    }

    public void DumpView(string filePath)
    {
        ValidatePath(filePath);
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_dump(_handle, Path.GetFullPath(filePath)));
    }

    public OcctPoint3d ScreenToWorld(int x, int y)
    {
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_screen_to_world(_handle, x, y, out var point));
        return point;
    }

    public Point WorldToScreen(OcctPoint3d point)
    {
        OcctGuard.Finite(point, nameof(point));
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_world_to_screen(_handle, point, out var x, out var y));
        return new Point(x, y);
    }

    public void StartRotation(int x, int y) =>
        Navigate(NavigationOptions(NativeViewerNavigationAction.StartRotation, x: x, y: y));

    public void Rotation(int x, int y) =>
        Navigate(NavigationOptions(NativeViewerNavigationAction.Rotation, x: x, y: y));

    public void Pan(int deltaX, int deltaY) =>
        Navigate(NavigationOptions(NativeViewerNavigationAction.Pan, deltaX: deltaX, deltaY: deltaY));

    public void Zoom(double factor)
    {
        OcctGuard.Positive(factor, nameof(factor));
        Navigate(NavigationOptions(NativeViewerNavigationAction.Zoom, factor: factor));
    }

    public OcctCameraState GetCamera()
    {
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_camera_get(_handle, out var result));
        return result;
    }

    public void SetCamera(OcctCameraState state)
    {
        OcctGuard.Finite(state.Eye, nameof(state.Eye));
        OcctGuard.Finite(state.Center, nameof(state.Center));
        OcctGuard.Positive(state.Scale, nameof(state.Scale));
        OcctGuard.NonZero(state.Up, nameof(state.Up));
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_camera_set(_handle, in state));
    }

    public double ViewScale
    {
        get => GetCamera().Scale;
        set
        {
            OcctGuard.Positive(value, nameof(value));
            UpdateViewState(ViewStateOptions(
                NativeViewerViewStateUpdateMask.Scale,
                scale: value));
        }
    }

    public void SetAntialiasing(bool enabled) =>
        UpdateViewState(ViewStateOptions(
            NativeViewerViewStateUpdateMask.Antialiasing,
            antialiasingEnabled: enabled));

    public void SetGradientBackground(
        Color first,
        Color second,
        OcctGradientFillMethod fillMethod = OcctGradientFillMethod.Vertical)
    {
        if (!Enum.IsDefined(fillMethod)) throw new ArgumentOutOfRangeException(nameof(fillMethod));
        UpdateViewState(ViewStateOptions(
            NativeViewerViewStateUpdateMask.GradientBackground,
            backgroundFirst: ToNativeViewColor(first),
            backgroundSecond: ToNativeViewColor(second),
            gradientFillMethod: (int)fillMethod));
    }

    private void UpdateViewState(NativeViewerViewStateOptions options)
    {
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_state_update(_handle, in options));
    }

    private void UpdateDisplayQuality(NativeViewerDisplayQualityOptions options)
    {
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_display_quality_update(_handle, in options));
    }

    private void Navigate(NativeViewerNavigationOptions options)
    {
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_navigation(_handle, in options));
    }

    private static NativeViewerViewStateOptions ViewStateOptions(
        NativeViewerViewStateUpdateMask updateMask,
        int orientation = 0,
        int projectionType = 0,
        double perspectiveFovDegrees = 45.0,
        NativeViewColorRgb backgroundFirst = default,
        NativeViewColorRgb backgroundSecond = default,
        int gradientFillMethod = 0,
        int displayMode = 0,
        bool triedronVisible = false,
        bool viewCubeVisible = false,
        bool computedMode = false,
        bool antialiasingEnabled = false,
        double scale = 1.0,
        bool fitAfterOrientation = false) => new()
    {
        StructSize = (uint)Marshal.SizeOf<NativeViewerViewStateOptions>(),
        ApiVersion = 1,
        UpdateMask = updateMask,
        Orientation = orientation,
        ProjectionType = projectionType,
        PerspectiveFovDegrees = perspectiveFovDegrees,
        BackgroundFirst = backgroundFirst,
        BackgroundSecond = backgroundSecond,
        GradientFillMethod = gradientFillMethod,
        DisplayMode = displayMode,
        TriedronVisible = triedronVisible ? 1 : 0,
        ViewCubeVisible = viewCubeVisible ? 1 : 0,
        ComputedMode = computedMode ? 1 : 0,
        AntialiasingEnabled = antialiasingEnabled ? 1 : 0,
        Scale = scale,
        FitAfterOrientation = fitAfterOrientation ? 1 : 0
    };

    private static NativeViewerNavigationOptions NavigationOptions(
        NativeViewerNavigationAction action,
        int x = 0,
        int y = 0,
        int deltaX = 0,
        int deltaY = 0,
        double factor = 1.0) => new()
    {
        StructSize = (uint)Marshal.SizeOf<NativeViewerNavigationOptions>(),
        ApiVersion = 1,
        Action = action,
        X = x,
        Y = y,
        DeltaX = deltaX,
        DeltaY = deltaY,
        Factor = factor
    };

    private static NativeViewColorRgb ToNativeViewColor(Color value) => new()
    {
        R = value.R / 255.0,
        G = value.G / 255.0,
        B = value.B / 255.0
    };

    private void CheckViewStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
