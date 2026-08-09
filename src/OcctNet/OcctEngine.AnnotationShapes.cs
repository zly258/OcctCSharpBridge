namespace OcctNet;

public sealed partial class OcctEngine
{
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
}
