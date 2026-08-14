namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_regular_polygon(OcctEngineSafeHandle handle, OcctPoint3d center, OcctVector3d normal, OcctVector3d xDirection, double radius, int sideCount, int makeFace);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_rectangle_wire(OcctEngineSafeHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_face_from_wire(OcctEngineSafeHandle handle, long wireId, int onlyPlane);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_make_plane_face(OcctEngineSafeHandle handle, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height);
}
