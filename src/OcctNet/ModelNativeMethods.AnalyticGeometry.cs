using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_line_geometry(OcctModelingSafeHandle handle, long edgeId, out OcctLineGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_circle_geometry(OcctModelingSafeHandle handle, long edgeId, out OcctCircleGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_edge_ellipse_geometry(OcctModelingSafeHandle handle, long edgeId, out OcctEllipseGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_plane_geometry(OcctModelingSafeHandle handle, long faceId, out OcctPlaneGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_cylinder_geometry(OcctModelingSafeHandle handle, long faceId, out OcctCylinderGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_cone_geometry(OcctModelingSafeHandle handle, long faceId, out OcctConeGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_sphere_geometry(OcctModelingSafeHandle handle, long faceId, out OcctSphereGeometry result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_face_torus_geometry(OcctModelingSafeHandle handle, long faceId, out OcctTorusGeometry result);
}
