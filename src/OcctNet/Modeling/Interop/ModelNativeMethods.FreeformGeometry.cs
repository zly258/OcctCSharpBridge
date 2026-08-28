using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_parabola_geometry(
        OcctModelingSafeHandle handle,
        long edgeId,
        out OcctParabolaGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_hyperbola_geometry(
        OcctModelingSafeHandle handle,
        long edgeId,
        out OcctHyperbolaGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_bezier_info(
        OcctModelingSafeHandle handle,
        long edgeId,
        out NativeBezierCurveInfo result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_bezier_poles_snapshot_get(
        OcctModelingSafeHandle handle,
        long edgeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] OcctPoint3d[]? poles,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] double[]? weights,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_bezier_info(
        OcctModelingSafeHandle handle,
        long faceId,
        out NativeBezierSurfaceInfo result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_bezier_poles_snapshot_get(
        OcctModelingSafeHandle handle,
        long faceId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] OcctPoint3d[]? poles,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] double[]? weights,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_extrusion_geometry(
        OcctModelingSafeHandle handle,
        long faceId,
        out OcctExtrusionSurfaceGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_revolution_geometry(
        OcctModelingSafeHandle handle,
        long faceId,
        out OcctRevolutionSurfaceGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_offset_geometry(
        OcctModelingSafeHandle handle,
        long faceId,
        out OcctOffsetSurfaceGeometry result);
}
