using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class OcafNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_naming_generated(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, long newShapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_naming_generated_from(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, long oldShapeId, long newShapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_naming_modify(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, long oldShapeId, long newShapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_naming_delete(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, long oldShapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_naming_select(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, long selectedShapeId, long contextShapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_named_shape_exists(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_named_shape_is_empty(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_named_shape_evolution(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_named_shape_version(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_named_shape_version(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int version);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_ocaf_named_shape_get(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_named_shape_pair_snapshot(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_ocaf_named_shape_old_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_ocaf_named_shape_new_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_selector_select(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, long selectedShapeId, long contextShapeId, int geometryMode);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_selector_solve(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_selector_is_identified(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, IntPtr model, long shapeId);
}
