using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewerHighlightUpdateMask : uint
{
    Selection = 1u << 0,
    Hover = 1u << 1
}

[Flags]
internal enum NativeViewerHighlightStyleUpdateMask : uint
{
    SelectionColor = 1u << 0,
    HoverColor = 1u << 1,
    SelectionMode = 1u << 2,
    HoverMode = 1u << 3
}

internal enum NativeHighlightMode
{
    BoundingBox = 0,
    Wireframe = 1,
    Shaded = 2
}

internal static partial class AppearanceNativeMethods
{
    private const string LibraryName = "OcctNative";

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeColorRgb
    {
        internal double R;
        internal double G;
        internal double B;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeSceneLightingSettings
    {
        internal NativeColorRgb AmbientColor;
        internal double AmbientIntensity;
        internal int CameraLightEnabled;
        internal NativeColorRgb CameraLightColor;
        internal double CameraLightIntensity;
        internal OcctVector3d CameraLightDirection;
        internal int SunLightEnabled;
        internal NativeColorRgb SunLightColor;
        internal double SunLightIntensity;
        internal OcctVector3d SunLightDirection;
        internal int FillLightEnabled;
        internal NativeColorRgb FillLightColor;
        internal double FillLightIntensity;
        internal OcctVector3d FillLightDirection;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeViewerLightingOptions
    {
        internal uint StructSize;
        internal uint ApiVersion;
        internal NativeSceneLightingSettings Settings;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeViewerHighlightOptions
    {
        internal uint StructSize;
        internal uint ApiVersion;
        internal NativeViewerHighlightUpdateMask UpdateMask;
        internal NativeColorRgb SelectionColor;
        internal NativeColorRgb HoverColor;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeViewerHighlightStyleOptions
    {
        internal uint StructSize;
        internal uint ApiVersion;
        internal NativeViewerHighlightStyleUpdateMask UpdateMask;
        internal NativeColorRgb SelectionColor;
        internal NativeColorRgb HoverColor;
        internal NativeHighlightMode SelectionMode;
        internal NativeHighlightMode HoverMode;
    }

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_scene_lighting_set(
        OcctEngineSafeHandle handle,
        in NativeViewerLightingOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_scene_lighting_reset(
        OcctEngineSafeHandle handle);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_highlight_colors_set(
        OcctEngineSafeHandle handle,
        in NativeViewerHighlightOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_highlight_style_set(
        OcctEngineSafeHandle handle,
        in NativeViewerHighlightStyleOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_object_line_style_set(
        OcctEngineSafeHandle handle,
        long objectId,
        int lineStyle);
}
