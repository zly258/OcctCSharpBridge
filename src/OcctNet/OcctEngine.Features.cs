using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape Boolean(OcctBooleanOperation operation, OcctShape left, OcctShape right, bool hideInputs = true)
    {
        if (!Enum.IsDefined(operation)) throw new ArgumentOutOfRangeException(nameof(operation));
        EnsureShape(left);
        EnsureShape(right);
        EnsureInitialized();
        var status = ViewerFeatureNativeMethods.occt_engine_shape_boolean(
            _handle,
            (int)operation,
            left.Id,
            right.Id,
            hideInputs ? 1 : 0,
            out var result);
        return FeatureResult(status, result);
    }

    public OcctShape Fuse(OcctShape left, OcctShape right, bool hideInputs = true) =>
        Boolean(OcctBooleanOperation.Fuse, left, right, hideInputs);

    public OcctShape Cut(OcctShape left, OcctShape right, bool hideInputs = true) =>
        Boolean(OcctBooleanOperation.Cut, left, right, hideInputs);

    public OcctShape Common(OcctShape left, OcctShape right, bool hideInputs = true) =>
        Boolean(OcctBooleanOperation.Common, left, right, hideInputs);

    public OcctShape Section(OcctShape left, OcctShape right, bool hideInputs = false) =>
        Boolean(OcctBooleanOperation.Section, left, right, hideInputs);

    public OcctShape Extrude(OcctShape profile, OcctVector3d vector, bool hideInput = true)
    {
        EnsureShape(profile);
        OcctGuard.NonZero(vector, nameof(vector));
        EnsureInitialized();
        var status = ViewerFeatureNativeMethods.occt_engine_shape_extrude(
            _handle,
            profile.Id,
            vector,
            hideInput ? 1 : 0,
            out var result);
        return FeatureResult(status, result);
    }

    public OcctShape Revolve(
        OcctShape profile,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees = 360,
        bool hideInput = true)
    {
        EnsureShape(profile);
        OcctGuard.Finite(axisPoint, nameof(axisPoint));
        OcctGuard.NonZero(axisDirection, nameof(axisDirection));
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        if (Math.Abs(angleDegrees) <= double.Epsilon)
            throw new ArgumentOutOfRangeException(nameof(angleDegrees), angleDegrees, "Revolve angle must be non-zero.");
        EnsureInitialized();
        var status = ViewerFeatureNativeMethods.occt_engine_shape_revolve(
            _handle,
            profile.Id,
            axisPoint,
            axisDirection,
            angleDegrees,
            hideInput ? 1 : 0,
            out var result);
        return FeatureResult(status, result);
    }

    public OcctShape Sweep(OcctShape spineWire, OcctShape profile, bool hideInputs = true)
    {
        EnsureShape(spineWire);
        EnsureShape(profile);
        EnsureInitialized();
        var status = ViewerFeatureNativeMethods.occt_engine_shape_sweep(
            _handle,
            spineWire.Id,
            profile.Id,
            hideInputs ? 1 : 0,
            out var result);
        return FeatureResult(status, result);
    }

    public OcctShape Loft(
        IEnumerable<OcctShape> sectionWires,
        bool makeSolid = true,
        bool ruled = false,
        double tolerance = 1e-6,
        bool hideInputs = true)
    {
        var ids = ShapeIds(sectionWires);
        if (ids.Length < 2) throw new ArgumentException("Loft requires at least two section wires.", nameof(sectionWires));
        OcctGuard.Positive(tolerance, nameof(tolerance));
        EnsureInitialized();

        var buffer = Marshal.AllocHGlobal(checked(sizeof(long) * ids.Length));
        try
        {
            Marshal.Copy(ids, 0, buffer, ids.Length);
            var status = ViewerFeatureNativeMethods.occt_engine_shape_loft(
                _handle,
                buffer,
                ids.Length,
                makeSolid ? 1 : 0,
                ruled ? 1 : 0,
                tolerance,
                hideInputs ? 1 : 0,
                out var result);
            return FeatureResult(status, result);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public OcctShape FilletAllEdges(OcctShape shape, double radius, bool hideInput = true)
    {
        EnsureShape(shape);
        OcctGuard.Positive(radius, nameof(radius));
        EnsureInitialized();
        var status = ViewerFeatureNativeMethods.occt_engine_shape_fillet_all_edges(
            _handle,
            shape.Id,
            radius,
            hideInput ? 1 : 0,
            out var result);
        return FeatureResult(status, result);
    }

    public OcctShape FilletEdges(OcctShape shape, IEnumerable<int> edgeIndices, double radius, bool hideInput = true)
    {
        EnsureShape(shape);
        OcctGuard.Positive(radius, nameof(radius));
        var indices = NormalizeEdgeIndices(edgeIndices, nameof(edgeIndices));
        EnsureInitialized();
        return IndexedFeature(
            indices,
            (IntPtr buffer, int count, out long result) => ViewerFeatureNativeMethods.occt_engine_shape_fillet_edges(
                _handle,
                shape.Id,
                buffer,
                count,
                radius,
                hideInput ? 1 : 0,
                out result));
    }

    public OcctShape ChamferEdges(OcctShape shape, IEnumerable<int> edgeIndices, double distance, bool hideInput = true)
    {
        EnsureShape(shape);
        OcctGuard.Positive(distance, nameof(distance));
        var indices = NormalizeEdgeIndices(edgeIndices, nameof(edgeIndices));
        EnsureInitialized();
        return IndexedFeature(
            indices,
            (IntPtr buffer, int count, out long result) => ViewerFeatureNativeMethods.occt_engine_shape_chamfer_edges(
                _handle,
                shape.Id,
                buffer,
                count,
                distance,
                hideInput ? 1 : 0,
                out result));
    }

    public OcctShape ChamferAllEdges(OcctShape shape, double distance, bool hideInput = true)
    {
        EnsureShape(shape);
        OcctGuard.Positive(distance, nameof(distance));
        EnsureInitialized();
        var status = ViewerFeatureNativeMethods.occt_engine_shape_chamfer_all_edges(
            _handle,
            shape.Id,
            distance,
            hideInput ? 1 : 0,
            out var result);
        return FeatureResult(status, result);
    }

    public OcctShape Offset(OcctShape shape, double offset, double tolerance = 1e-4, bool hideInput = true)
    {
        EnsureShape(shape);
        OcctGuard.Finite(offset, nameof(offset));
        OcctGuard.Positive(tolerance, nameof(tolerance));
        EnsureInitialized();
        var status = ViewerFeatureNativeMethods.occt_engine_shape_offset(
            _handle,
            shape.Id,
            offset,
            tolerance,
            hideInput ? 1 : 0,
            out var result);
        return FeatureResult(status, result);
    }

    public OcctShape MakeThickSolid(
        OcctShape solid,
        int faceIndexToRemove,
        double thickness,
        double tolerance = 1e-4,
        bool hideInput = true)
    {
        EnsureShape(solid);
        OcctGuard.PositiveIndex(faceIndexToRemove, nameof(faceIndexToRemove));
        OcctGuard.Finite(thickness, nameof(thickness));
        if (Math.Abs(thickness) <= double.Epsilon)
            throw new ArgumentOutOfRangeException(nameof(thickness), thickness, "Thickness must be non-zero.");
        OcctGuard.Positive(tolerance, nameof(tolerance));
        EnsureInitialized();
        var status = ViewerFeatureNativeMethods.occt_engine_shape_thick_solid(
            _handle,
            solid.Id,
            faceIndexToRemove,
            thickness,
            tolerance,
            hideInput ? 1 : 0,
            out var result);
        return FeatureResult(status, result);
    }

    public OcctShape AddBoss(OcctShape baseShape, OcctShape profile, OcctVector3d vector, bool hideInputs = true)
    {
        EnsureShape(baseShape);
        EnsureShape(profile);
        var tool = Extrude(profile, vector, hideInput: hideInputs);
        return Fuse(baseShape, tool, hideInputs);
    }

    public OcctShape AddPocket(OcctShape baseShape, OcctShape profile, OcctVector3d vector, bool hideInputs = true)
    {
        EnsureShape(baseShape);
        EnsureShape(profile);
        var tool = Extrude(profile, vector, hideInput: hideInputs);
        return Cut(baseShape, tool, hideInputs);
    }

    public OcctShape DrillHole(
        OcctShape baseShape,
        OcctPoint3d origin,
        OcctVector3d axis,
        double radius,
        double depth,
        bool hideInput = true)
    {
        EnsureShape(baseShape);
        var tool = MakeCylinder(origin, axis, radius, depth);
        return Cut(baseShape, tool, hideInputs: hideInput);
    }

    private delegate OcctStatus IndexedFeatureCall(IntPtr indices, int count, out long result);

    private OcctShape IndexedFeature(int[] indices, IndexedFeatureCall call)
    {
        var buffer = Marshal.AllocHGlobal(checked(sizeof(int) * indices.Length));
        try
        {
            Marshal.Copy(indices, 0, buffer, indices.Length);
            var status = call(buffer, indices.Length, out var result);
            return FeatureResult(status, result);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static int[] NormalizeEdgeIndices(IEnumerable<int> edgeIndices, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(edgeIndices);
        var indices = edgeIndices.Distinct().ToArray();
        if (indices.Length == 0) throw new ArgumentException("Collection must not be empty.", parameterName);
        foreach (var index in indices) OcctGuard.PositiveIndex(index, parameterName);
        return indices;
    }

    private OcctShape FeatureResult(OcctStatus status, long result)
    {
        if (status != OcctStatus.Ok) throw CreateException();
        return CheckShape(result);
    }
}
