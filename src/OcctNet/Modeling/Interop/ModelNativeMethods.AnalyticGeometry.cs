using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_surface_plane_face_create(OcctModelingSafeHandle handle, OcctPoint3d origin, OcctVector3d normal, OcctVector3d xDirection, double uMin, double uMax, double vMin, double vMax, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_surface_cylinder_face_create(OcctModelingSafeHandle handle, OcctPoint3d origin, OcctVector3d axis, OcctVector3d xDirection, double radius, double uMin, double uMax, double vMin, double vMax, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_surface_cone_face_create(OcctModelingSafeHandle handle, OcctPoint3d referenceOrigin, OcctVector3d axis, OcctVector3d xDirection, double referenceRadius, double semiAngleRadians, double uMin, double uMax, double vMin, double vMax, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_surface_sphere_face_create(OcctModelingSafeHandle handle, OcctPoint3d center, OcctVector3d axis, OcctVector3d xDirection, double radius, double uMin, double uMax, double vMin, double vMax, out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_surface_torus_face_create(OcctModelingSafeHandle handle, OcctPoint3d center, OcctVector3d axis, OcctVector3d xDirection, double majorRadius, double minorRadius, double uMin, double uMax, double vMin, double vMax, out long result);

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
