namespace OcctNet;

public sealed partial class OcctEngine
{
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
