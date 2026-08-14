using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_brep_text_create(
        OcctModelingSafeHandle session,
        string utf8Text,
        string fontName,
        in NativeBRepTextOptions options,
        out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_length_annotation_create(
        OcctModelingSafeHandle session,
        long edgeId,
        string fontName,
        in NativeBRepAnnotationOptions options,
        out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_angle_annotation_create(
        OcctModelingSafeHandle session,
        long firstEdgeId,
        long secondEdgeId,
        string fontName,
        in NativeBRepAnnotationOptions options,
        out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_radius_annotation_create(
        OcctModelingSafeHandle session,
        long circularEdgeId,
        string fontName,
        in NativeBRepAnnotationOptions options,
        out long resultShapeId);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial OcctStatus occt_model_diameter_annotation_create(
        OcctModelingSafeHandle session,
        long circularEdgeId,
        string fontName,
        in NativeBRepAnnotationOptions options,
        out long resultShapeId);
}
