using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeGltfExportOptions
{
    public uint StructSize;
    public uint ApiVersion;
    public int WriteBinary;        // OcctBool
    public int TransformToGltfCs;  // OcctBool
    public double Deflection;
}

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_obj_import(
        OcctModelingSafeHandle session, string path, out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_obj_export(
        OcctModelingSafeHandle session, long shapeId, string path);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_gltf_import(
        OcctModelingSafeHandle session, string path, out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_gltf_export(
        OcctModelingSafeHandle session, long shapeId, string path,
        in NativeGltfExportOptions options);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_stl_export_multiple(
        OcctModelingSafeHandle session,
        [In] long[] shapeIds,
        int count,
        string path,
        in NativeStlExportOptions options);
}
