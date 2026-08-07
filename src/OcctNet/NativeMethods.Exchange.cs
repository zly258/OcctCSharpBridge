namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_import_file(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_import_step(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_import_iges(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_import_brep(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_import_stl(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_step(IntPtr handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_all_step(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_iges(IntPtr handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_all_iges(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_brep(IntPtr handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_stl(IntPtr handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, double linearDeflection, double angularDeflection, int asciiMode);
}
