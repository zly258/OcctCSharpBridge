using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_vertex_point(OcctModelingSafeHandle handle, long vertexId, out OcctPoint3d result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_endpoints(OcctModelingSafeHandle handle, long edgeId, out OcctPoint3d start, out OcctPoint3d end);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_point_at(OcctModelingSafeHandle handle, long edgeId, double normalizedParameter, out OcctPoint3d point, out OcctVector3d tangent);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_length(OcctModelingSafeHandle handle, long edgeId, out double result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_length_at_parameter(OcctModelingSafeHandle handle, long edgeId, double parameter, out double result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_point_at_length(OcctModelingSafeHandle handle, long edgeId, double length, out double curveParameter, out OcctPoint3d resultPoint, out OcctVector3d resultTangent);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_sample_points_snapshot_get(
        OcctModelingSafeHandle handle,
        long edgeId,
        int sampleCount,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] OcctPoint3d[]? results,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_curve_type(OcctModelingSafeHandle handle, long edgeId, out OcctCurveType result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_surface_type(OcctModelingSafeHandle handle, long faceId, out OcctSurfaceType result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_uv_bounds(OcctModelingSafeHandle handle, long faceId, out OcctUvBounds result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_point_normal(OcctModelingSafeHandle handle, long faceId, double u, double v, out OcctPoint3d point, out OcctVector3d normal);
}
