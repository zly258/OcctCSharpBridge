using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeStlExportOptions
{
    public uint StructSize;
    public uint ApiVersion;
    public double LinearDeflection;
    public double AngularDeflection;
    public int Ascii;
}

internal static partial class ModelNativeMethods
{
    // Current ABI. High-level SDK code must use these status-based entry points.
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_file_import(
        OcctModelingSafeHandle session,
        string path,
        out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_step_import(
        OcctModelingSafeHandle session,
        string path,
        out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_iges_import(
        OcctModelingSafeHandle session,
        string path,
        out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_brep_import(
        OcctModelingSafeHandle session,
        string path,
        out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_stl_import(
        OcctModelingSafeHandle session,
        string path,
        out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_step_export(
        OcctModelingSafeHandle session,
        long shapeId,
        string path);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_iges_export(
        OcctModelingSafeHandle session,
        long shapeId,
        string path);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_brep_export(
        OcctModelingSafeHandle session,
        long shapeId,
        string path);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_stl_export(
        OcctModelingSafeHandle session,
        long shapeId,
        string path,
        in NativeStlExportOptions options);

    // Frozen ABI 4 declarations. These exist only for compatibility accounting while
    // migration is in progress; production high-level APIs do not call them.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_import_file(OcctModelingSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_import_step(OcctModelingSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_import_iges(OcctModelingSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_import_brep(OcctModelingSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern long occt_model_import_stl(OcctModelingSafeHandle handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_export_step(OcctModelingSafeHandle handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_export_iges(OcctModelingSafeHandle handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_export_brep(OcctModelingSafeHandle handle, long shapeId, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_export_stl(
        OcctModelingSafeHandle handle,
        long shapeId,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        double linearDeflection,
        double angularDeflection,
        int asciiMode);
}
