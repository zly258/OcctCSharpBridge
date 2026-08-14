using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_line_geometry(OcctModelingSafeHandle handle, long edgeId, out OcctLineGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_circle_geometry(OcctModelingSafeHandle handle, long edgeId, out OcctCircleGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_ellipse_geometry(OcctModelingSafeHandle handle, long edgeId, out OcctEllipseGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_plane_geometry(OcctModelingSafeHandle handle, long faceId, out OcctPlaneGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_cylinder_geometry(OcctModelingSafeHandle handle, long faceId, out OcctCylinderGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_cone_geometry(OcctModelingSafeHandle handle, long faceId, out OcctConeGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_sphere_geometry(OcctModelingSafeHandle handle, long faceId, out OcctSphereGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_torus_geometry(OcctModelingSafeHandle handle, long faceId, out OcctTorusGeometry result);
}
