namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelAlgorithmResult Extrude(OcctModelShape profile, OcctVector3d vector)
    {
        EnsureShape(profile);
        OcctGuard.NonZero(vector, nameof(vector));
        var status = ModelNativeMethods.occt_model_feature_extrude_execute(
            _handle, profile.Id, vector, out var result);
        return CheckAlgorithm(status, result);
    }

    public OcctModelAlgorithmResult Revolve(
        OcctModelShape profile,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees = 360)
    {
        EnsureShape(profile);
        OcctGuard.Finite(axisPoint, nameof(axisPoint));
        OcctGuard.NonZero(axisDirection, nameof(axisDirection));
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        if (Math.Abs(angleDegrees) <= double.Epsilon)
            throw new ArgumentOutOfRangeException(nameof(angleDegrees), angleDegrees, "Revolve angle must be non-zero.");
        var status = ModelNativeMethods.occt_model_feature_revolve_execute(
            _handle,
            profile.Id,
            axisPoint,
            axisDirection,
            angleDegrees,
            out var result);
        return CheckAlgorithm(status, result);
    }

    public OcctModelAlgorithmResult Sweep(OcctModelShape spineWire, OcctModelShape profile)
    {
        EnsureShape(spineWire);
        EnsureShape(profile);
        var status = ModelNativeMethods.occt_model_feature_sweep_execute(
            _handle, spineWire.Id, profile.Id, out var result);
        return CheckAlgorithm(status, result);
    }

    public OcctModelAlgorithmResult Loft(
        IEnumerable<OcctModelShape> sectionWires,
        bool makeSolid = true,
        bool ruled = false,
        double tolerance = 1e-6)
    {
        var ids = ShapeIds(sectionWires);
        if (ids.Length < 2)
            throw new ArgumentException("Loft requires at least two section wires.", nameof(sectionWires));
        OcctGuard.Positive(tolerance, nameof(tolerance));
        var status = ModelNativeMethods.occt_model_feature_loft_execute(
            _handle,
            ids,
            ids.Length,
            makeSolid ? 1 : 0,
            ruled ? 1 : 0,
            tolerance,
            out var result);
        return CheckAlgorithm(status, result);
    }

    public OcctModelAlgorithmResult FilletEdges(
        OcctModelShape shape,
        IEnumerable<int> edgeIndices,
        double radius)
    {
        EnsureShape(shape);
        OcctGuard.Positive(radius, nameof(radius));
        var indices = RequiredArray(edgeIndices, nameof(edgeIndices)).Distinct().ToArray();
        foreach (var index in indices) OcctGuard.PositiveIndex(index, nameof(edgeIndices));
        var status = ModelNativeMethods.occt_model_feature_fillet_edges_execute(
            _handle,
            shape.Id,
            indices,
            indices.Length,
            radius,
            out var result);
        return CheckAlgorithm(status, result);
    }

    public OcctModelAlgorithmResult ChamferEdges(
        OcctModelShape shape,
        IEnumerable<int> edgeIndices,
        double distance)
    {
        EnsureShape(shape);
        OcctGuard.Positive(distance, nameof(distance));
        var indices = RequiredArray(edgeIndices, nameof(edgeIndices)).Distinct().ToArray();
        foreach (var index in indices) OcctGuard.PositiveIndex(index, nameof(edgeIndices));
        var status = ModelNativeMethods.occt_model_feature_chamfer_edges_execute(
            _handle,
            shape.Id,
            indices,
            indices.Length,
            distance,
            out var result);
        return CheckAlgorithm(status, result);
    }

    public OcctModelAlgorithmResult OffsetShape(
        OcctModelShape shape,
        double offset,
        double tolerance = 1e-4)
    {
        EnsureShape(shape);
        OcctGuard.Finite(offset, nameof(offset));
        if (Math.Abs(offset) <= 1e-15)
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be non-zero.");
        OcctGuard.Positive(tolerance, nameof(tolerance));
        var status = ModelNativeMethods.occt_model_feature_offset_execute(
            _handle,
            shape.Id,
            offset,
            tolerance,
            out var result);
        return CheckAlgorithm(status, result);
    }

    public OcctModelAlgorithmResult MakeThickSolid(
        OcctModelShape solid,
        IEnumerable<int> faceIndicesToRemove,
        double thickness,
        double tolerance = 1e-4)
    {
        EnsureShape(solid);
        OcctGuard.Finite(thickness, nameof(thickness));
        if (Math.Abs(thickness) <= double.Epsilon)
            throw new ArgumentOutOfRangeException(nameof(thickness), thickness, "Thickness must be non-zero.");
        OcctGuard.Positive(tolerance, nameof(tolerance));
        var indices = RequiredArray(faceIndicesToRemove, nameof(faceIndicesToRemove)).Distinct().ToArray();
        foreach (var index in indices) OcctGuard.PositiveIndex(index, nameof(faceIndicesToRemove));
        var status = ModelNativeMethods.occt_model_feature_thick_solid_execute(
            _handle,
            solid.Id,
            indices,
            indices.Length,
            thickness,
            tolerance,
            out var result);
        return CheckAlgorithm(status, result);
    }
}
