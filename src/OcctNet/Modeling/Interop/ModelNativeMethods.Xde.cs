using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_step_document_import(
        OcctModelingSafeHandle handle,
        string path,
        out long primaryShapeId,
        out IntPtr document);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_iges_document_import(
        OcctModelingSafeHandle handle,
        string path,
        out long primaryShapeId,
        out IntPtr document);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_obj_document_import(
        OcctModelingSafeHandle handle,
        string path,
        out long primaryShapeId,
        out IntPtr document);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_gltf_document_import(
        OcctModelingSafeHandle handle,
        string path,
        out long primaryShapeId,
        out IntPtr document);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void occt_xde_document_release(IntPtr document);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_step_document_export(
        OcctModelingSafeHandle handle,
        OcctXdeDocumentSafeHandle document,
        string path);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_iges_document_export(
        OcctModelingSafeHandle handle,
        OcctXdeDocumentSafeHandle document,
        string path);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_xde_document_json_get(
        OcctModelingSafeHandle handle,
        OcctXdeDocumentSafeHandle document,
        [Out] byte[]? utf8Buffer,
        int capacity,
        out int requiredBytes);
}
