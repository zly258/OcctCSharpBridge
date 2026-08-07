using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_application_tag(IntPtr handle, long objectId, [MarshalAs(UnmanagedType.LPUTF8Str)] string applicationTag);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr occt_get_object_application_tag(IntPtr handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_find_object_by_application_tag(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string applicationTag);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_update_object_shape_from_model(IntPtr engineHandle, IntPtr modelHandle, long viewerObjectId, long modelShapeId, uint options);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_selectable(IntPtr handle, long objectId, int selectable);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_selectable(IntPtr handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_selectable(IntPtr handle, long[] objectIds, int count, int selectable);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_selected_objects_ex(IntPtr handle, long[]? objectIds, int count, int operation);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_transform(IntPtr handle, long objectId, [In] double[] matrix3x4);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_transform(IntPtr handle, long objectId, [Out] double[] matrix3x4, out int hasTransform);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_reset_object_transform(IntPtr handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_view_cube_language(IntPtr handle, int language);
}
