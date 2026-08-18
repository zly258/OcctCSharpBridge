using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewerTextUpdateMask : uint
{
    None = 0,
    Content = 1u << 0,
    Position = 1u << 1,
    Height = 1u << 2,
    Font = 1u << 3,
    Angle = 1u << 4,
    Zoomable = 1u << 5,
    Color = 1u << 6
}

internal enum NativeViewerDimensionKind
{
    Length = 0,
    Angle = 1,
    Radius = 2,
    Diameter = 3
}

[Flags]
internal enum NativeViewerDimensionUpdateMask : uint
{
    None = 0,
    Flyout = 1u << 0,
    Color = 1u << 1
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerTextOptions
{
    public uint StructSize;
    public uint ApiVersion;
    public NativeViewerTextUpdateMask UpdateMask;
    public OcctPoint3d Position;
    public double Height;
    public double AngleDegrees;
    public double Red;
    public double Green;
    public double Blue;
    public int Zoomable;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerDimensionOptions
{
    public uint StructSize;
    public uint ApiVersion;
    public NativeViewerDimensionUpdateMask UpdateMask;
    public double Flyout;
    public double Red;
    public double Green;
    public double Blue;
}

internal static partial class AnnotationNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_text_create(
        OcctEngineSafeHandle engine,
        string utf8Text,
        string fontName,
        in NativeViewerTextOptions options,
        out long resultTextId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_text_update(
        OcctEngineSafeHandle engine,
        long textId,
        string utf8Text,
        string fontName,
        in NativeViewerTextOptions options);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_dimension_create(
        OcctEngineSafeHandle engine,
        NativeViewerDimensionKind kind,
        long firstShapeId,
        long secondShapeId,
        in NativeViewerDimensionOptions options,
        out long resultDimensionId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_length_dimension_create_in_plane(
        OcctEngineSafeHandle engine,
        long edgeShapeId,
        OcctVector3d planeNormal,
        in NativeViewerDimensionOptions options,
        out long resultDimensionId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_angle_dimension_create_in_plane(
        OcctEngineSafeHandle engine,
        long firstEdgeShapeId,
        long secondEdgeShapeId,
        OcctVector3d planeNormal,
        in NativeViewerDimensionOptions options,
        out long resultDimensionId);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_dimension_update(
        OcctEngineSafeHandle engine,
        long dimensionId,
        in NativeViewerDimensionOptions options);
}
