using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_hash(
        OcctModelingSafeHandle handle,
        long shapeId,
        out long result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_type(
        OcctModelingSafeHandle handle,
        long shapeId,
        out OcctShapeType result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_orientation(
        OcctModelingSafeHandle handle,
        long shapeId,
        out OcctModelOrientation result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_is_closed(
        OcctModelingSafeHandle handle,
        long shapeId,
        out int result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_is_valid(
        OcctModelingSafeHandle handle,
        long shapeId,
        out int result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_tolerance(
        OcctModelingSafeHandle handle,
        long shapeId,
        out double result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_bounds(
        OcctModelingSafeHandle handle,
        long shapeId,
        out OcctBounds result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_linear_properties(
        OcctModelingSafeHandle handle,
        long shapeId,
        out OcctMassProperties result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_surface_properties(
        OcctModelingSafeHandle handle,
        long shapeId,
        out OcctMassProperties result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_volume_properties(
        OcctModelingSafeHandle handle,
        long shapeId,
        out OcctMassProperties result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_distance(
        OcctModelingSafeHandle handle,
        long firstId,
        long secondId,
        out OcctDistanceResult result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_distances(
        OcctModelingSafeHandle handle,
        [In, MarshalUsing(CountElementName = nameof(count))] long[] firstIds,
        [In, MarshalUsing(CountElementName = nameof(count))] long[] secondIds,
        int count,
        [Out, MarshalUsing(CountElementName = nameof(count))] OcctDistanceResult[] results);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_check_report_get(
        OcctModelingSafeHandle handle,
        long shapeId,
        [Out, MarshalUsing(CountElementName = nameof(capacity))] byte[]? buffer,
        int capacity,
        out int required);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_location_get(
        OcctModelingSafeHandle handle,
        long shapeId,
        out OcctModelLocation result);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial OcctStatus occt_model_shape_location_set(
        OcctModelingSafeHandle handle,
        long shapeId,
        in OcctModelLocation location,
        int copyShape,
        out long result);
}
