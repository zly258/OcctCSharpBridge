using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewerViewStateUpdateMask : uint
{
    Orientation = 1u << 0,
    Projection = 1u << 1,
    PerspectiveFov = 1u << 2,
    SolidBackground = 1u << 3,
    GradientBackground = 1u << 4,
    DisplayMode = 1u << 5,
    TriedronVisible = 1u << 6,
    ViewCubeVisible = 1u << 7,
    ComputedMode = 1u << 8,
    Antialiasing = 1u << 9,
    Scale = 1u << 10
}

[Flags]
internal enum NativeViewerDisplayQualityUpdateMask : uint
{
    Precision = 1u << 0,
    DefaultMaterial = 1u << 1
}

internal enum NativeViewerNavigationAction
{
    StartRotation = 0,
    Rotation = 1,
    Pan = 2,
    Zoom = 3
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewColorRgb
{
    internal double R;
    internal double G;
    internal double B;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerViewStateOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerViewStateUpdateMask UpdateMask;
    internal int Orientation;
    internal int ProjectionType;
    internal double PerspectiveFovDegrees;
    internal NativeViewColorRgb BackgroundFirst;
    internal NativeViewColorRgb BackgroundSecond;
    internal int GradientFillMethod;
    internal int DisplayMode;
    internal int TriedronVisible;
    internal int ViewCubeVisible;
    internal int ComputedMode;
    internal int AntialiasingEnabled;
    internal double Scale;
    internal int FitAfterOrientation;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerDisplayQualityOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerDisplayQualityUpdateMask UpdateMask;
    internal double DeviationCoefficient;
    internal double DeviationAngleDegrees;
    internal int Material;
    internal int ApplyPrecisionToExisting;
    internal int ApplyMaterialToExisting;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerNavigationOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerNavigationAction Action;
    internal int X;
    internal int Y;
    internal int DeltaX;
    internal int DeltaY;
    internal double Factor;
}

internal static partial class ViewNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_state_update(
        OcctEngineSafeHandle handle,
        in NativeViewerViewStateOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_display_quality_update(
        OcctEngineSafeHandle handle,
        in NativeViewerDisplayQualityOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_camera_get(
        OcctEngineSafeHandle handle,
        out OcctCameraState result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_camera_set(
        OcctEngineSafeHandle handle,
        in OcctCameraState state);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_fit_all(OcctEngineSafeHandle handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_fit_object(
        OcctEngineSafeHandle handle,
        long objectId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_window_fit(
        OcctEngineSafeHandle handle,
        int x1,
        int y1,
        int x2,
        int y2);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_screen_to_world(
        OcctEngineSafeHandle handle,
        int x,
        int y,
        out OcctPoint3d result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_projection_ray(
        OcctEngineSafeHandle handle,
        int x,
        int y,
        out OcctProjectionRay result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_world_to_screen(
        OcctEngineSafeHandle handle,
        OcctPoint3d point,
        out int x,
        out int y);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_navigation(
        OcctEngineSafeHandle handle,
        in NativeViewerNavigationOptions options);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_dump(
        OcctEngineSafeHandle handle,
        string path);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_cube_language_set(
        OcctEngineSafeHandle handle,
        int language);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_cube_axes_set(
        OcctEngineSafeHandle handle,
        int visible);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_view_cube_try_click(
        OcctEngineSafeHandle handle,
        int x,
        int y,
        out int handled);
}
