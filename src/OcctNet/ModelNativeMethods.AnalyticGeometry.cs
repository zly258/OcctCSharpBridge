using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_model_edge_line_geometry(IntPtr handle, long edgeId, out OcctLineGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_model_edge_circle_geometry(IntPtr handle, long edgeId, out OcctCircleGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_model_edge_ellipse_geometry(IntPtr handle, long edgeId, out OcctEllipseGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_model_face_plane_geometry(IntPtr handle, long faceId, out OcctPlaneGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_model_face_cylinder_geometry(IntPtr handle, long faceId, out OcctCylinderGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_model_face_cone_geometry(IntPtr handle, long faceId, out OcctConeGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_model_face_sphere_geometry(IntPtr handle, long faceId, out OcctSphereGeometry result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int occt_model_face_torus_geometry(IntPtr handle, long faceId, out OcctTorusGeometry result);
}
