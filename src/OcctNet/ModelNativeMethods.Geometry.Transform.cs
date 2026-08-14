using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_translate(OcctModelingSafeHandle handle, long shapeId, OcctVector3d vector);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_rotate(OcctModelingSafeHandle handle, long shapeId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_scale(OcctModelingSafeHandle handle, long shapeId, OcctPoint3d center, double factor);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_model_mirror_plane(OcctModelingSafeHandle handle, long shapeId, OcctPoint3d planePoint, OcctVector3d planeNormal);
}
