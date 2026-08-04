using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class OcafNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_name(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_name(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, out IntPtr value);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_comment(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_comment(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, out IntPtr value);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_ascii_string(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_ascii_string(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, out IntPtr value);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_integer(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int value);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_integer(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, out int value);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_real(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, double value);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_real(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, out double value);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_uattribute(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string guid);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_has_uattribute(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string guid);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_reference(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, [MarshalAs(UnmanagedType.LPUTF8Str)] string targetEntry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_reference(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, out IntPtr targetEntry);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_integer_array(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int[] values, int count, int lower);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_integer_array(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_real_array(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, double[] values, int count, int lower);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_real_array(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_boolean_array(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int[] values, int count, int lower);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_boolean_array(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_byte_array(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, byte[] values, int count, int lower);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_byte_array(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_string_array(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, IntPtr[] values, int count, int lower);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_string_array(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_array_lower(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_array_count(IntPtr handle);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_array_int_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern double occt_ocaf_array_real_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr occt_ocaf_array_string_at(IntPtr handle, int index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_position(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, OcctPoint3d point);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_get_position(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, out OcctPoint3d point);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern int occt_ocaf_set_shape_attribute(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, long shapeId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)] internal static extern long occt_ocaf_get_shape_attribute(IntPtr handle, IntPtr model, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry);
}
