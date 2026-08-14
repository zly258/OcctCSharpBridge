using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewerHighlightUpdateMask : uint
{
    Selection = 1u << 0,
    Hover = 1u << 1
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

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_scene_lighting_set(
        OcctEngineSafeHandle handle,
        in NativeViewerLightingOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_highlight_colors_set(
        OcctEngineSafeHandle handle,
        in NativeViewerHighlightOptions options);

    // Frozen ABI 4 declarations retained only for compatibility-surface verification.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_scene_lighting_ex(
        OcctEngineSafeHandle handle,
        in NativeSceneLightingSettings settings);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_selection_highlight_color(
        OcctEngineSafeHandle handle,
        double r,
        double g,
        double b);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_hover_highlight_color(
        OcctEngineSafeHandle handle,
        double r,
        double g,
        double b);
}
