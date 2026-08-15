using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerSelectionRectangleOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal int X1;
    internal int Y1;
    internal int X2;
    internal int Y2;
    internal NativeViewColorRgb LineColor;
    internal NativeViewColorRgb FillColor;
    internal double FillTransparency;
    internal double LineWidth;
}

internal static partial class SelectionOverlayNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_rectangle_overlay_show(
        OcctEngineSafeHandle handle,
        in NativeViewerSelectionRectangleOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_selection_rectangle_overlay_hide(
        OcctEngineSafeHandle handle);
}
