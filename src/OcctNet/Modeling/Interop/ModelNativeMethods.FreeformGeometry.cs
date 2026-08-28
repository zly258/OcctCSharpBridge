using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    internal static partial OcctStatus occt_model_edge_bezier_pole_at(
        OcctModelingSafeHandle handle,
        long edgeId,
        int index,
        out OcctPoint3d pole,
        out double weight);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_bezier_info(
        OcctModelingSafeHandle handle,
        long faceId,
        out NativeBezierSurfaceInfo result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_bezier_pole_at(
        OcctModelingSafeHandle handle,
        long faceId,
        int uIndex,
        int vIndex,
        out OcctPoint3d pole,
        out double weight);

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
