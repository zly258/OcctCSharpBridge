using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_application_tag(OcctEngineSafeHandle handle, long objectId, [MarshalAs(UnmanagedType.LPUTF8Str)] string applicationTag);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr occt_get_object_application_tag(OcctEngineSafeHandle handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_find_object_by_application_tag(OcctEngineSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string applicationTag);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_update_object_shape_from_model(OcctEngineSafeHandle engineHandle, OcctModelingSafeHandle modelHandle, long viewerObjectId, long modelShapeId, uint options);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_selectable(OcctEngineSafeHandle handle, long objectId, int selectable);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_selectable(OcctEngineSafeHandle handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_objects_selectable(OcctEngineSafeHandle handle, long[] objectIds, int count, int selectable);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_selected_objects_ex(OcctEngineSafeHandle handle, long[]? objectIds, int count, int operation);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_object_transform(OcctEngineSafeHandle handle, long objectId, [In] double[] matrix3x4);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_get_object_transform(OcctEngineSafeHandle handle, long objectId, [Out] double[] matrix3x4, out int hasTransform);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_reset_object_transform(OcctEngineSafeHandle handle, long objectId);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_set_view_cube_language(OcctEngineSafeHandle handle, int language);
}
