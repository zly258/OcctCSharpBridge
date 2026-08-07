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

    /// <summary>Creates vector BRep text that remains geometrically sharp at any zoom level.</summary>
    public OcctShape MakeTextShape(
        string text,
        OcctPoint3d position,
        double height = 16,
        double extrusionDepth = 0,
        string? fontName = null,
        OcctVector3d? normal = null,
        OcctVector3d? xDirection = null,
        bool bold = false,
        bool italic = false)
    {
        OcctGuard.Finite(position, nameof(position));
        OcctGuard.Positive(height, nameof(height));
        OcctGuard.Finite(extrusionDepth, nameof(extrusionDepth));
        var actualNormal = normal ?? OcctVector3d.UnitZ;
        var actualXDirection = xDirection ?? OcctVector3d.UnitX;
        OcctGuard.NonZero(actualNormal, nameof(normal));
        OcctGuard.NonZero(actualXDirection, nameof(xDirection));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_text_shape(
            _handle,
            text ?? string.Empty,
            position,
            actualNormal,
            actualXDirection,
            height,
            extrusionDepth,
            fontName ?? string.Empty,
            bold ? 1 : 0,
            italic ? 1 : 0));
    }

    /// <summary>Creates a result-only BRep linear annotation, including vector text and arrows.</summary>
    public OcctShape MakeLengthAnnotationShape(OcctShape edge, double flyout = 20, double textHeight = 8, double arrowSize = 5, string? fontName = null)
    {
        EnsureShape(edge);
        OcctGuard.Finite(flyout, nameof(flyout));
        OcctGuard.Positive(textHeight, nameof(textHeight));
        OcctGuard.Positive(arrowSize, nameof(arrowSize));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_length_annotation_shape(
            _handle, edge.Id, flyout, textHeight, arrowSize, fontName ?? string.Empty));
    }

    /// <summary>Creates a result-only BRep angular annotation, including vector text and arrows.</summary>
    public OcctShape MakeAngleAnnotationShape(OcctShape firstEdge, OcctShape secondEdge, double radius = 30, double textHeight = 8, double arrowSize = 5, string? fontName = null)
    {
        EnsureShape(firstEdge);
        EnsureShape(secondEdge);
        OcctGuard.Positive(radius, nameof(radius));
        OcctGuard.Positive(textHeight, nameof(textHeight));
        OcctGuard.Positive(arrowSize, nameof(arrowSize));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_angle_annotation_shape(
            _handle, firstEdge.Id, secondEdge.Id, radius, textHeight, arrowSize, fontName ?? string.Empty));
    }

    /// <summary>Creates a result-only BRep radius annotation, including vector text and an arrow.</summary>
    public OcctShape MakeRadiusAnnotationShape(OcctShape circularEdge, double flyout = 20, double textHeight = 8, double arrowSize = 5, string? fontName = null)
    {
        EnsureShape(circularEdge);
        OcctGuard.Finite(flyout, nameof(flyout));
        OcctGuard.Positive(textHeight, nameof(textHeight));
        OcctGuard.Positive(arrowSize, nameof(arrowSize));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_radius_annotation_shape(
            _handle, circularEdge.Id, flyout, textHeight, arrowSize, fontName ?? string.Empty));
    }

    /// <summary>Creates a result-only BRep diameter annotation, including vector text and arrows.</summary>
    public OcctShape MakeDiameterAnnotationShape(OcctShape circularEdge, double flyout = 20, double textHeight = 8, double arrowSize = 5, string? fontName = null)
    {
        EnsureShape(circularEdge);
        OcctGuard.Finite(flyout, nameof(flyout));
        OcctGuard.Positive(textHeight, nameof(textHeight));
        OcctGuard.Positive(arrowSize, nameof(arrowSize));
        EnsureInitialized();
        return CheckShape(NativeMethods.occt_make_diameter_annotation_shape(
            _handle, circularEdge.Id, flyout, textHeight, arrowSize, fontName ?? string.Empty));
    }

    public OcctText AddText(string text, OcctPoint3d position, double height = 16, System.Drawing.Color? color = null, bool zoomable = true)
    {
        OcctGuard.Finite(position, nameof(position));
        OcctGuard.Positive(height, nameof(height));
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        return CheckText(NativeMethods.occt_add_text(_handle, text ?? string.Empty, position, height, actualColor.R / 255.0, actualColor.G / 255.0, actualColor.B / 255.0, zoomable ? 1 : 0));
    }

    public void SetText(OcctText textObject, string text)
    {
        EnsureText(textObject);
        CheckInitialized(() => NativeMethods.occt_set_text(_handle, textObject.Id, text ?? string.Empty));
    }

    public void SetTextPosition(OcctText textObject, OcctPoint3d position)
    {
        EnsureText(textObject);
        OcctGuard.Finite(position, nameof(position));
        CheckInitialized(() => NativeMethods.occt_set_text_position(_handle, textObject.Id, position));
    }

    public void SetTextHeight(OcctText textObject, double height)
    {
        EnsureText(textObject);
        OcctGuard.Positive(height, nameof(height));
        CheckInitialized(() => NativeMethods.occt_set_text_height(_handle, textObject.Id, height));
    }

    public void SetTextFont(OcctText textObject, string fontName)
    {
        EnsureText(textObject);
        CheckInitialized(() => NativeMethods.occt_set_text_font(_handle, textObject.Id, fontName ?? string.Empty));
    }

    public void SetTextAngle(OcctText textObject, double angleDegrees)
    {
        EnsureText(textObject);
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        CheckInitialized(() => NativeMethods.occt_set_text_angle(_handle, textObject.Id, angleDegrees));
    }

    public void SetTextZoomable(OcctText textObject, bool zoomable)
    {
        EnsureText(textObject);
        CheckInitialized(() => NativeMethods.occt_set_text_zoomable(_handle, textObject.Id, zoomable ? 1 : 0));
    }

    public void SetDimensionFlyout(OcctDimension dimension, double flyout)
    {
        EnsureDimension(dimension);
        OcctGuard.Finite(flyout, nameof(flyout));
        CheckInitialized(() => NativeMethods.occt_set_dimension_flyout(_handle, dimension.Id, flyout));
    }

    public OcctDimension AddLengthDimension(OcctShape edge, double flyout = 20, System.Drawing.Color? color = null)
    {
        EnsureShape(edge);
        OcctGuard.Finite(flyout, nameof(flyout));
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        return CheckDimension(NativeMethods.occt_add_length_dimension(_handle, edge.Id, flyout, actualColor.R / 255.0, actualColor.G / 255.0, actualColor.B / 255.0));
    }

    public OcctDimension AddAngleDimension(OcctShape firstEdge, OcctShape secondEdge, double flyout = 20, System.Drawing.Color? color = null)
    {
        EnsureShape(firstEdge);
        EnsureShape(secondEdge);
        OcctGuard.Finite(flyout, nameof(flyout));
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        return CheckDimension(NativeMethods.occt_add_angle_dimension(_handle, firstEdge.Id, secondEdge.Id, flyout, actualColor.R / 255.0, actualColor.G / 255.0, actualColor.B / 255.0));
    }

    public OcctDimension AddRadiusDimension(OcctShape circularShape, double flyout = 20, System.Drawing.Color? color = null)
    {
        EnsureShape(circularShape);
        OcctGuard.Finite(flyout, nameof(flyout));
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        return CheckDimension(NativeMethods.occt_add_radius_dimension(_handle, circularShape.Id, flyout, actualColor.R / 255.0, actualColor.G / 255.0, actualColor.B / 255.0));
    }

    public OcctDimension AddDiameterDimension(OcctShape circularShape, double flyout = 20, System.Drawing.Color? color = null)
    {
        EnsureShape(circularShape);
        OcctGuard.Finite(flyout, nameof(flyout));
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        return CheckDimension(NativeMethods.occt_add_diameter_dimension(_handle, circularShape.Id, flyout, actualColor.R / 255.0, actualColor.G / 255.0, actualColor.B / 255.0));
    }
}
