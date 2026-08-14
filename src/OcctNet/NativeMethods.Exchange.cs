namespace OcctNet;

internal static partial class NativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_import_file(OcctEngineSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_import_step(OcctEngineSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_import_iges(OcctEngineSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_import_brep(OcctEngineSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern long occt_import_stl(OcctEngineSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_step(OcctEngineSafeHandle handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_all_step(OcctEngineSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_iges(OcctEngineSafeHandle handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_all_iges(OcctEngineSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_brep(OcctEngineSafeHandle handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)] internal static extern int occt_export_stl(OcctEngineSafeHandle handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, double linearDeflection, double angularDeflection, int asciiMode);
}
