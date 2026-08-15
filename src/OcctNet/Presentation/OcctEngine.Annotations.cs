using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctText AddText(string text, OcctPoint3d position, double height = 16, System.Drawing.Color? color = null, bool zoomable = true)
    {
        OcctGuard.Finite(position, nameof(position));
        OcctGuard.Positive(height, nameof(height));
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        var options = TextOptions(
            NativeViewerTextUpdateMask.Content |
            NativeViewerTextUpdateMask.Position |
            NativeViewerTextUpdateMask.Height |
            NativeViewerTextUpdateMask.Angle |
            NativeViewerTextUpdateMask.Zoomable |
            NativeViewerTextUpdateMask.Color,
            position,
            height,
            0,
            actualColor,
            zoomable);
        var status = AnnotationNativeMethods.occt_engine_text_create(
            _handle,
            text ?? string.Empty,
            string.Empty,
            in options,
            out var textId);
        if (status != OcctStatus.Ok) throw CreateException();
        return CheckText(textId);
    }

    public void SetText(OcctText textObject, string text)
    {
        EnsureText(textObject);
        var options = TextOptions(NativeViewerTextUpdateMask.Content);
        CheckAnnotationStatus(AnnotationNativeMethods.occt_engine_text_update(
            _handle, textObject.Id, text ?? string.Empty, string.Empty, in options));
    }

    public void SetTextPosition(OcctText textObject, OcctPoint3d position)
    {
        EnsureText(textObject);
        OcctGuard.Finite(position, nameof(position));
        var options = TextOptions(NativeViewerTextUpdateMask.Position, position: position);
        CheckAnnotationStatus(AnnotationNativeMethods.occt_engine_text_update(
            _handle, textObject.Id, string.Empty, string.Empty, in options));
    }

    public void SetTextHeight(OcctText textObject, double height)
    {
        EnsureText(textObject);
        OcctGuard.Positive(height, nameof(height));
        var options = TextOptions(NativeViewerTextUpdateMask.Height, height: height);
        CheckAnnotationStatus(AnnotationNativeMethods.occt_engine_text_update(
            _handle, textObject.Id, string.Empty, string.Empty, in options));
    }

    public void SetTextFont(OcctText textObject, string fontName)
    {
        EnsureText(textObject);
        var options = TextOptions(NativeViewerTextUpdateMask.Font);
        CheckAnnotationStatus(AnnotationNativeMethods.occt_engine_text_update(
            _handle, textObject.Id, string.Empty, fontName ?? string.Empty, in options));
    }

    public void SetTextAngle(OcctText textObject, double angleDegrees)
    {
        EnsureText(textObject);
        OcctGuard.Finite(angleDegrees, nameof(angleDegrees));
        var options = TextOptions(NativeViewerTextUpdateMask.Angle, angleDegrees: angleDegrees);
        CheckAnnotationStatus(AnnotationNativeMethods.occt_engine_text_update(
            _handle, textObject.Id, string.Empty, string.Empty, in options));
    }

    public void SetTextZoomable(OcctText textObject, bool zoomable)
    {
        EnsureText(textObject);
        var options = TextOptions(NativeViewerTextUpdateMask.Zoomable, zoomable: zoomable);
        CheckAnnotationStatus(AnnotationNativeMethods.occt_engine_text_update(
            _handle, textObject.Id, string.Empty, string.Empty, in options));
    }

    public void SetDimensionFlyout(OcctDimension dimension, double flyout)
    {
        EnsureDimension(dimension);
        OcctGuard.Finite(flyout, nameof(flyout));
        var options = DimensionOptions(NativeViewerDimensionUpdateMask.Flyout, flyout);
        CheckAnnotationStatus(AnnotationNativeMethods.occt_engine_dimension_update(_handle, dimension.Id, in options));
    }

    public OcctDimension AddLengthDimension(OcctShape edge, double flyout = 20, System.Drawing.Color? color = null)
    {
        EnsureShape(edge);
        return AddDimension(NativeViewerDimensionKind.Length, edge.Id, 0, flyout, color);
    }

    public OcctDimension AddAngleDimension(OcctShape firstEdge, OcctShape secondEdge, double flyout = 20, System.Drawing.Color? color = null)
    {
        EnsureShape(firstEdge);
        EnsureShape(secondEdge);
        return AddDimension(NativeViewerDimensionKind.Angle, firstEdge.Id, secondEdge.Id, flyout, color);
    }

    public OcctDimension AddRadiusDimension(OcctShape circularShape, double flyout = 20, System.Drawing.Color? color = null)
    {
        EnsureShape(circularShape);
        return AddDimension(NativeViewerDimensionKind.Radius, circularShape.Id, 0, flyout, color);
    }

    public OcctDimension AddDiameterDimension(OcctShape circularShape, double flyout = 20, System.Drawing.Color? color = null)
    {
        EnsureShape(circularShape);
        return AddDimension(NativeViewerDimensionKind.Diameter, circularShape.Id, 0, flyout, color);
    }

    private OcctDimension AddDimension(
        NativeViewerDimensionKind kind,
        long firstShapeId,
        long secondShapeId,
        double flyout,
        System.Drawing.Color? color)
    {
        OcctGuard.Finite(flyout, nameof(flyout));
        var actualColor = color ?? System.Drawing.Color.Black;
        EnsureInitialized();
        var options = DimensionOptions(
            NativeViewerDimensionUpdateMask.Flyout | NativeViewerDimensionUpdateMask.Color,
            flyout,
            actualColor);
        var status = AnnotationNativeMethods.occt_engine_dimension_create(
            _handle,
            kind,
            firstShapeId,
            secondShapeId,
            in options,
            out var dimensionId);
        if (status != OcctStatus.Ok) throw CreateException();
        return CheckDimension(dimensionId);
    }

    private static NativeViewerTextOptions TextOptions(
        NativeViewerTextUpdateMask updateMask,
        OcctPoint3d position = default,
        double height = 1,
        double angleDegrees = 0,
        System.Drawing.Color? color = null,
        bool zoomable = false)
    {
        var actualColor = color ?? System.Drawing.Color.Black;
        return new NativeViewerTextOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerTextOptions>(),
            ApiVersion = 1,
            UpdateMask = updateMask,
            Position = position,
            Height = height,
            AngleDegrees = angleDegrees,
            Red = actualColor.R / 255.0,
            Green = actualColor.G / 255.0,
            Blue = actualColor.B / 255.0,
            Zoomable = zoomable ? 1 : 0
        };
    }

    private static NativeViewerDimensionOptions DimensionOptions(
        NativeViewerDimensionUpdateMask updateMask,
        double flyout,
        System.Drawing.Color? color = null)
    {
        var actualColor = color ?? System.Drawing.Color.Black;
        return new NativeViewerDimensionOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerDimensionOptions>(),
            ApiVersion = 1,
            UpdateMask = updateMask,
            Flyout = flyout,
            Red = actualColor.R / 255.0,
            Green = actualColor.G / 255.0,
            Blue = actualColor.B / 255.0
        };
    }

    private void CheckAnnotationStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}
