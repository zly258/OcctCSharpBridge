using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_bspline_info(
        OcctModelingSafeHandle handle,
        long edgeId,
        out OcctModelBSplineCurveInfoNative result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_bspline_poles_snapshot_get(
        OcctModelingSafeHandle handle,
        long edgeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] OcctPoint3d[]? poles,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] double[]? weights,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_bspline_knots_snapshot_get(
        OcctModelingSafeHandle handle,
        long edgeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] double[]? knots,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] int[]? multiplicities,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_bspline_info(
        OcctModelingSafeHandle handle,
        long faceId,
        out OcctModelBSplineSurfaceInfoNative result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_bspline_poles_snapshot_get(
        OcctModelingSafeHandle handle,
        long faceId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] OcctPoint3d[]? poles,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] double[]? weights,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_bspline_u_knots_snapshot_get(
        OcctModelingSafeHandle handle,
        long faceId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] double[]? knots,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] int[]? multiplicities,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_bspline_v_knots_snapshot_get(
        OcctModelingSafeHandle handle,
        long faceId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] double[]? knots,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] int[]? multiplicities,
        int capacity,
        out int required);
}
