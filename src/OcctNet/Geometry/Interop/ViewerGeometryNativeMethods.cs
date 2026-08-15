using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

[Flags]
internal enum NativeViewerIndexedEdgeQueryMask : uint
{
    Endpoints = 1u << 0,
    Evaluation = 1u << 1
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerIndexedEdgeQueryOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerIndexedEdgeQueryMask QueryMask;
    internal int EdgeIndex;
    internal double NormalizedParameter;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerIndexedEdgeQueryResult
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal OcctPoint3d Start;
    internal OcctPoint3d End;
    internal OcctPoint3d Point;
    internal OcctVector3d Tangent;
}

[Flags]
internal enum NativeViewerIndexedFaceQueryMask : uint
{
    Evaluation = 1u << 0,
    Center = 1u << 1
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerIndexedFaceQueryOptions
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal NativeViewerIndexedFaceQueryMask QueryMask;
    internal int FaceIndex;
    internal double U;
    internal double V;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeViewerIndexedFaceQueryResult
{
    internal uint StructSize;
    internal uint ApiVersion;
    internal OcctPoint3d Point;
    internal OcctVector3d Normal;
    internal OcctPoint3d Center;
}

internal static partial class ViewerGeometryNativeMethods
{
    private const string LibraryName = "OcctNative";

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_indexed_vertex_get(
        OcctEngineSafeHandle handle,
        long ownerId,
        int vertexIndex,
        out OcctPoint3d result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_indexed_edge_query(
        OcctEngineSafeHandle handle,
        long ownerId,
        in NativeViewerIndexedEdgeQueryOptions options,
        out NativeViewerIndexedEdgeQueryResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_engine_indexed_face_query(
        OcctEngineSafeHandle handle,
        long ownerId,
        in NativeViewerIndexedFaceQueryOptions options,
        out NativeViewerIndexedFaceQueryResult result);
}
