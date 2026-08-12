namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape Boolean(OcctBooleanOperation operation, OcctShape left, OcctShape right, bool hideInputs = true)
    {
        if (!Enum.IsDefined(operation)) throw new ArgumentOutOfRangeException(nameof(operation));
        EnsureShape(left);
        EnsureShape(right);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_boolean(_handle, (int)operation, left.Id, right.Id, hideInputs ? 1 : 0));
    }

    public OcctShape Fuse(OcctShape left, OcctShape right, bool hideInputs = true) => Boolean(OcctBooleanOperation.Fuse, left, right, hideInputs);
    public OcctShape Cut(OcctShape left, OcctShape right, bool hideInputs = true) => Boolean(OcctBooleanOperation.Cut, left, right, hideInputs);
    public OcctShape Common(OcctShape left, OcctShape right, bool hideInputs = true) => Boolean(OcctBooleanOperation.Common, left, right, hideInputs);
    public OcctShape Section(OcctShape left, OcctShape right, bool hideInputs = false) => Boolean(OcctBooleanOperation.Section, left, right, hideInputs);

    public OcctShape Extrude(OcctShape profile, OcctVector3d vector, bool hideInput = true)
    {
        EnsureShape(profile);
        OcctGuard.NonZero(vector, nameof(vector));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_extrude(_handle, profile.Id, vector, hideInput ? 1 : 0));
    }

    public OcctShape Revolve(OcctShape profile, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees = 360, bool hideInput = true)
    {
        EnsureShape(profile);
        OcctGuard.Finite(axisPoint, nameof(axisPoint));
        OcctGuard.NonZero(axisDirection, nameof(axisDirection));
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        if (Math.Abs(angleDegrees) <= double.Epsilon)
            throw new ArgumentOutOfRangeException(nameof(angleDegrees), angleDegrees, "Revolve angle must be non-zero.");
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_revolve(_handle, profile.Id, axisPoint, axisDirection, angleDegrees, hideInput ? 1 : 0));
    }

    public OcctShape Sweep(OcctShape spineWire, OcctShape profile, bool hideInputs = true)
    {
        EnsureShape(spineWire);
        EnsureShape(profile);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_sweep(_handle, spineWire.Id, profile.Id, hideInputs ? 1 : 0));
    }

    public OcctShape Loft(IEnumerable<OcctShape> sectionWires, bool makeSolid = true, bool ruled = false, double tolerance = 1e-6, bool hideInputs = true)
    {
        var ids = ShapeIds(sectionWires);
        if (ids.Length < 2) throw new ArgumentException("Loft requires at least two section wires.", nameof(sectionWires));
        OcctGuard.Positive(tolerance, nameof(tolerance));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_loft(_handle, ids, ids.Length, makeSolid ? 1 : 0, ruled ? 1 : 0, tolerance, hideInputs ? 1 : 0));
    }

    public OcctShape FilletAllEdges(OcctShape shape, double radius, bool hideInput = true)
    {
        EnsureShape(shape);
        OcctGuard.Positive(radius, nameof(radius));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_fillet_all_edges(_handle, shape.Id, radius, hideInput ? 1 : 0));
    }

    public OcctShape FilletEdges(OcctShape shape, IEnumerable<int> edgeIndices, double radius, bool hideInput = true)
    {
        EnsureShape(shape);
        OcctGuard.Positive(radius, nameof(radius));
        ArgumentNullException.ThrowIfNull(edgeIndices);
        var indices = edgeIndices.Distinct().ToArray();
        if (indices.Length == 0) throw new ArgumentException("Collection must not be empty.", nameof(edgeIndices));
        foreach (var index in indices) OcctGuard.PositiveIndex(index, nameof(edgeIndices));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_fillet_edges(_handle, shape.Id, indices, indices.Length, radius, hideInput ? 1 : 0));
    }

    public OcctShape ChamferEdges(OcctShape shape, IEnumerable<int> edgeIndices, double distance, bool hideInput = true)
    {
        EnsureShape(shape);
        OcctGuard.Positive(distance, nameof(distance));
        ArgumentNullException.ThrowIfNull(edgeIndices);
        var indices = edgeIndices.Distinct().ToArray();
        if (indices.Length == 0) throw new ArgumentException("Collection must not be empty.", nameof(edgeIndices));
        foreach (var index in indices) OcctGuard.PositiveIndex(index, nameof(edgeIndices));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_chamfer_edges(_handle, shape.Id, indices, indices.Length, distance, hideInput ? 1 : 0));
    }

    public OcctShape ChamferAllEdges(OcctShape shape, double distance, bool hideInput = true)
    {
        EnsureShape(shape);
        OcctGuard.Positive(distance, nameof(distance));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_chamfer_all_edges(_handle, shape.Id, distance, hideInput ? 1 : 0));
    }

    public OcctShape Offset(OcctShape shape, double offset, double tolerance = 1e-4, bool hideInput = true)
    {
        EnsureShape(shape);
        OcctGuard.Finite(offset, nameof(offset));
        OcctGuard.Positive(tolerance, nameof(tolerance));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_offset_shape(_handle, shape.Id, offset, tolerance, hideInput ? 1 : 0));
    }

    public OcctShape MakeThickSolid(OcctShape solid, int faceIndexToRemove, double thickness, double tolerance = 1e-4, bool hideInput = true)
    {
        EnsureShape(solid);
        OcctGuard.PositiveIndex(faceIndexToRemove, nameof(faceIndexToRemove));
        OcctGuard.Finite(thickness, nameof(thickness));
        if (Math.Abs(thickness) <= double.Epsilon)
            throw new ArgumentOutOfRangeException(nameof(thickness), thickness, "Thickness must be non-zero.");
        OcctGuard.Positive(tolerance, nameof(tolerance));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_thick_solid(_handle, solid.Id, faceIndexToRemove, thickness, tolerance, hideInput ? 1 : 0));
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

    public OcctShape DrillHole(OcctShape baseShape, OcctPoint3d origin, OcctVector3d axis, double radius, double depth, bool hideInput = true)
    {
        EnsureShape(baseShape);
        var tool = MakeCylinder(origin, axis, radius, depth);
        return Cut(baseShape, tool, hideInputs: hideInput);
    }
}
