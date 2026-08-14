using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_shape_linear_inertia(OcctModelingSafeHandle handle, long shapeId, out NativeModelInertiaProperties result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_shape_surface_inertia(OcctModelingSafeHandle handle, long shapeId, out NativeModelInertiaProperties result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_shape_volume_inertia(OcctModelingSafeHandle handle, long shapeId, out NativeModelInertiaProperties result);
}
