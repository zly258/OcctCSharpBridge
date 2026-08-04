namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape Boolean(OcctBooleanOperation operation, OcctShape left, OcctShape right, bool hideInputs = true)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_boolean(_handle, (int)operation, left.Id, right.Id, hideInputs ? 1 : 0));
    }

    public OcctShape Fuse(OcctShape left, OcctShape right, bool hideInputs = true) => Boolean(OcctBooleanOperation.Fuse, left, right, hideInputs);
    public OcctShape Cut(OcctShape left, OcctShape right, bool hideInputs = true) => Boolean(OcctBooleanOperation.Cut, left, right, hideInputs);
    public OcctShape Common(OcctShape left, OcctShape right, bool hideInputs = true) => Boolean(OcctBooleanOperation.Common, left, right, hideInputs);
    public OcctShape Section(OcctShape left, OcctShape right, bool hideInputs = false) => Boolean(OcctBooleanOperation.Section, left, right, hideInputs);

    public OcctShape Extrude(OcctShape profile, OcctVector3d vector, bool hideInput = true)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_extrude(_handle, profile.Id, vector, hideInput ? 1 : 0));
    }

    public OcctShape Revolve(OcctShape profile, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees = 360, bool hideInput = true)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_revolve(_handle, profile.Id, axisPoint, axisDirection, angleDegrees, hideInput ? 1 : 0));
    }

    public OcctShape Sweep(OcctShape spineWire, OcctShape profile, bool hideInputs = true)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_sweep(_handle, spineWire.Id, profile.Id, hideInputs ? 1 : 0));
    }

    public OcctShape Loft(IEnumerable<OcctShape> sectionWires, bool makeSolid = true, bool ruled = false, double tolerance = 1e-6, bool hideInputs = true)
    {
        var ids = ShapeIds(sectionWires);
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_loft(_handle, ids, ids.Length, makeSolid ? 1 : 0, ruled ? 1 : 0, tolerance, hideInputs ? 1 : 0));
    }

    public OcctShape FilletAllEdges(OcctShape shape, double radius, bool hideInput = true)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_fillet_all_edges(_handle, shape.Id, radius, hideInput ? 1 : 0));
    }

    public OcctShape FilletEdges(OcctShape shape, IEnumerable<int> edgeIndices, double radius, bool hideInput = true)
    {
        ArgumentNullException.ThrowIfNull(edgeIndices);
        var indices = edgeIndices.Distinct().ToArray();
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_fillet_edges(_handle, shape.Id, indices, indices.Length, radius, hideInput ? 1 : 0));
    }

    public OcctShape ChamferEdges(OcctShape shape, IEnumerable<int> edgeIndices, double distance, bool hideInput = true)
    {
        ArgumentNullException.ThrowIfNull(edgeIndices);
        var indices = edgeIndices.Distinct().ToArray();
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_chamfer_edges(_handle, shape.Id, indices, indices.Length, distance, hideInput ? 1 : 0));
    }

    public OcctShape ChamferAllEdges(OcctShape shape, double distance, bool hideInput = true)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_chamfer_all_edges(_handle, shape.Id, distance, hideInput ? 1 : 0));
    }

    public OcctShape Offset(OcctShape shape, double offset, double tolerance = 1e-4, bool hideInput = true)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_offset_shape(_handle, shape.Id, offset, tolerance, hideInput ? 1 : 0));
    }

    public OcctShape MakeThickSolid(OcctShape solid, int faceIndexToRemove, double thickness, double tolerance = 1e-4, bool hideInput = true)
    {
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_thick_solid(_handle, solid.Id, faceIndexToRemove, thickness, tolerance, hideInput ? 1 : 0));
    }

    public OcctShape AddBoss(OcctShape baseShape, OcctShape profile, OcctVector3d vector, bool hideInputs = true)
    {
        var tool = Extrude(profile, vector, hideInput: hideInputs);
        return Fuse(baseShape, tool, hideInputs);
    }

    public OcctShape AddPocket(OcctShape baseShape, OcctShape profile, OcctVector3d vector, bool hideInputs = true)
    {
        var tool = Extrude(profile, vector, hideInput: hideInputs);
        return Cut(baseShape, tool, hideInputs);
    }

    public OcctShape DrillHole(OcctShape baseShape, OcctPoint3d origin, OcctVector3d axis, double radius, double depth, bool hideInput = true)
    {
        var tool = MakeCylinder(origin, axis, radius, depth);
        return Cut(baseShape, tool, hideInputs: hideInput);
    }

    public OcctText AddText(string text, OcctPoint3d position, double height = 16, System.Drawing.Color? color = null, bool zoomable = true)
    {
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        return CheckText(NativeMethods.occt_add_text(_handle, text ?? string.Empty, position, height, actualColor.R / 255.0, actualColor.G / 255.0, actualColor.B / 255.0, zoomable ? 1 : 0));
    }

    public void SetText(OcctText textObject, string text) => CheckInitialized(() => NativeMethods.occt_set_text(_handle, textObject.Id, text ?? string.Empty));
    public void SetTextPosition(OcctText textObject, OcctPoint3d position) => CheckInitialized(() => NativeMethods.occt_set_text_position(_handle, textObject.Id, position));
    public void SetTextHeight(OcctText textObject, double height) => CheckInitialized(() => NativeMethods.occt_set_text_height(_handle, textObject.Id, height));
    public void SetTextFont(OcctText textObject, string fontName) => CheckInitialized(() => NativeMethods.occt_set_text_font(_handle, textObject.Id, fontName ?? string.Empty));
    public void SetTextAngle(OcctText textObject, double angleDegrees) => CheckInitialized(() => NativeMethods.occt_set_text_angle(_handle, textObject.Id, angleDegrees));
    public void SetTextZoomable(OcctText textObject, bool zoomable) => CheckInitialized(() => NativeMethods.occt_set_text_zoomable(_handle, textObject.Id, zoomable ? 1 : 0));
    public void SetDimensionFlyout(OcctDimension dimension, double flyout) => CheckInitialized(() => NativeMethods.occt_set_dimension_flyout(_handle, dimension.Id, flyout));

    public OcctDimension AddLengthDimension(OcctShape edge, double flyout = 20, System.Drawing.Color? color = null)
    {
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        return CheckDimension(NativeMethods.occt_add_length_dimension(_handle, edge.Id, flyout, actualColor.R / 255.0, actualColor.G / 255.0, actualColor.B / 255.0));
    }

    public OcctDimension AddAngleDimension(OcctShape firstEdge, OcctShape secondEdge, double flyout = 20, System.Drawing.Color? color = null)
    {
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        return CheckDimension(NativeMethods.occt_add_angle_dimension(_handle, firstEdge.Id, secondEdge.Id, flyout, actualColor.R / 255.0, actualColor.G / 255.0, actualColor.B / 255.0));
    }

    public OcctDimension AddRadiusDimension(OcctShape circularShape, double flyout = 20, System.Drawing.Color? color = null)
    {
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        return CheckDimension(NativeMethods.occt_add_radius_dimension(_handle, circularShape.Id, flyout, actualColor.R / 255.0, actualColor.G / 255.0, actualColor.B / 255.0));
    }

    public OcctDimension AddDiameterDimension(OcctShape circularShape, double flyout = 20, System.Drawing.Color? color = null)
    {
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        return CheckDimension(NativeMethods.occt_add_diameter_dimension(_handle, circularShape.Id, flyout, actualColor.R / 255.0, actualColor.G / 255.0, actualColor.B / 255.0));
    }
}
