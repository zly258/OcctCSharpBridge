using System.Runtime.InteropServices;

namespace OcctNet;

internal static class AppearanceNativeMethods
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
