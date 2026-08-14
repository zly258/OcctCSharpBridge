using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelShape MakeBRepText(string text, OcctBRepTextOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        OcctGuard.Finite(options.Position, nameof(options.Position));
        OcctGuard.NonZero(options.Normal, nameof(options.Normal));
        OcctGuard.NonZero(options.XDirection, nameof(options.XDirection));
        OcctGuard.Positive(options.Height, nameof(options.Height));
        if (!double.IsFinite(options.ExtrusionDepth) || options.ExtrusionDepth < 0)
            throw new ArgumentOutOfRangeException(nameof(options.ExtrusionDepth), options.ExtrusionDepth, "Extrusion depth must be finite and non-negative.");

        EnsureNotDisposed();
        var native = new NativeBRepTextOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeBRepTextOptions>(),
            ApiVersion = 1,
            Position = options.Position,
            Normal = options.Normal,
            XDirection = options.XDirection,
            Height = options.Height,
            ExtrusionDepth = options.ExtrusionDepth,
            Bold = options.Bold ? 1 : 0,
            Italic = options.Italic ? 1 : 0,
            HorizontalAlignment = (int)options.HorizontalAlignment,
            VerticalAlignment = (int)options.VerticalAlignment
        };
        var status = ModelNativeMethods.occt_model_brep_text_create(
            _handle,
            text,
            options.FontName ?? string.Empty,
            in native,
            out var shapeId);
        return CheckStatusShape(status, shapeId, nameof(MakeBRepText));
    }

    public OcctModelShape MakeLengthAnnotation(
        OcctModelShape edge,
        OcctBRepAnnotationOptions options)
    {
        EnsureShape(edge);
        var native = ToNative(options);
        var status = ModelNativeMethods.occt_model_length_annotation_create(
            _handle,
            edge.Id,
            options.FontName ?? string.Empty,
            in native,
            out var shapeId);
        return CheckStatusShape(status, shapeId, nameof(MakeLengthAnnotation));
    }

    public OcctModelShape MakeAngleAnnotation(
        OcctModelShape firstEdge,
        OcctModelShape secondEdge,
        OcctBRepAnnotationOptions options)
    {
        EnsureShape(firstEdge);
        EnsureShape(secondEdge);
        if (options.Offset <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.Offset), options.Offset, "Angular annotation radius must be positive.");
        var native = ToNative(options);
        var status = ModelNativeMethods.occt_model_angle_annotation_create(
            _handle,
            firstEdge.Id,
            secondEdge.Id,
            options.FontName ?? string.Empty,
            in native,
            out var shapeId);
        return CheckStatusShape(status, shapeId, nameof(MakeAngleAnnotation));
    }

    public OcctModelShape MakeRadiusAnnotation(
        OcctModelShape circularEdge,
        OcctBRepAnnotationOptions options)
    {
        EnsureShape(circularEdge);
        var native = ToNative(options);
        var status = ModelNativeMethods.occt_model_radius_annotation_create(
            _handle,
            circularEdge.Id,
            options.FontName ?? string.Empty,
            in native,
            out var shapeId);
        return CheckStatusShape(status, shapeId, nameof(MakeRadiusAnnotation));
    }

    public OcctModelShape MakeDiameterAnnotation(
        OcctModelShape circularEdge,
        OcctBRepAnnotationOptions options)
    {
        EnsureShape(circularEdge);
        var native = ToNative(options);
        var status = ModelNativeMethods.occt_model_diameter_annotation_create(
            _handle,
            circularEdge.Id,
            options.FontName ?? string.Empty,
            in native,
            out var shapeId);
        return CheckStatusShape(status, shapeId, nameof(MakeDiameterAnnotation));
    }

    private static NativeBRepAnnotationOptions ToNative(OcctBRepAnnotationOptions options)
    {
        if (!double.IsFinite(options.Offset))
            throw new ArgumentOutOfRangeException(nameof(options.Offset), options.Offset, "Annotation offset must be finite.");
        OcctGuard.Positive(options.TextHeight, nameof(options.TextHeight));
        OcctGuard.Positive(options.ArrowSize, nameof(options.ArrowSize));
        return new NativeBRepAnnotationOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeBRepAnnotationOptions>(),
            ApiVersion = 1,
            Offset = options.Offset,
            TextHeight = options.TextHeight,
            ArrowSize = options.ArrowSize
        };
    }

    private OcctModelShape CheckStatusShape(OcctStatus status, long shapeId, string operation)
    {
        if (status != OcctStatus.Ok || shapeId <= 0)
            throw CreateException(operation);
        return new OcctModelShape(shapeId, _ownerId);
    }
}
