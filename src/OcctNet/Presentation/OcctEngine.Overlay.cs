using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public unsafe OcctOverlay AddOverlayLine(
        OcctPoint3d start,
        OcctPoint3d end,
        OcctOverlayLineStyle? style = null)
    {
        OcctGuard.Finite(start, nameof(start));
        OcctGuard.Finite(end, nameof(end));
        var actual = style ?? new OcctOverlayLineStyle();
        ValidateOverlayLineStyle(actual);
        var points = new[] { start, end };
        EnsureInitialized();
        fixed (OcctPoint3d* pointPtr = points)
        {
            var options = LineOptions(
                NativeOverlayLineUpdateMask.Geometry | NativeOverlayLineUpdateMask.Style,
                OcctOverlayPrimitiveType.Line,
                (IntPtr)pointPtr,
                points.Length,
                actual);
            CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_line_create(
                _handle,
                in options,
                out var overlayId));
            return CheckOverlay(overlayId, OcctOverlayPrimitiveType.Line);
        }
    }

    public unsafe OcctOverlay AddOverlayPolyline(
        IReadOnlyList<OcctPoint3d> points,
        OcctOverlayLineStyle? style = null)
    {
        var values = ValidateOverlayPoints(points, nameof(points));
        var actual = style ?? new OcctOverlayLineStyle();
        ValidateOverlayLineStyle(actual);
        EnsureInitialized();
        fixed (OcctPoint3d* pointPtr = values)
        {
            var options = LineOptions(
                NativeOverlayLineUpdateMask.Geometry | NativeOverlayLineUpdateMask.Style,
                OcctOverlayPrimitiveType.Polyline,
                (IntPtr)pointPtr,
                values.Length,
                actual);
            CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_line_create(
                _handle,
                in options,
                out var overlayId));
            return CheckOverlay(overlayId, OcctOverlayPrimitiveType.Polyline);
        }
    }

    public OcctOverlay AddOverlayMarker(
        OcctPoint3d position,
        OcctOverlayMarkerStyle? style = null)
    {
        OcctGuard.Finite(position, nameof(position));
        var actual = style ?? new OcctOverlayMarkerStyle();
        ValidateOverlayMarkerStyle(actual);
        EnsureInitialized();
        var options = MarkerOptions(
            NativeOverlayMarkerUpdateMask.Position | NativeOverlayMarkerUpdateMask.Style,
            position,
            actual);
        CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_marker_create(
            _handle,
            in options,
            out var overlayId));
        return CheckOverlay(overlayId, OcctOverlayPrimitiveType.Marker);
    }

    public OcctOverlay AddOverlayText(
        string text,
        OcctPoint3d position,
        OcctOverlayTextStyle? style = null)
    {
        OcctGuard.Finite(position, nameof(position));
        var actual = style ?? new OcctOverlayTextStyle();
        ValidateOverlayTextStyle(actual);
        EnsureInitialized();

        var textPtr = Marshal.StringToCoTaskMemUTF8(text ?? string.Empty);
        var fontPtr = Marshal.StringToCoTaskMemUTF8(actual.FontName);
        try
        {
            var options = TextOptions(
                NativeOverlayTextUpdateMask.Content |
                NativeOverlayTextUpdateMask.Position |
                NativeOverlayTextUpdateMask.Style,
                textPtr,
                position,
                actual,
                fontPtr);
            CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_text_create(
                _handle,
                in options,
                out var overlayId));
            return CheckOverlay(overlayId, OcctOverlayPrimitiveType.Text);
        }
        finally
        {
            Marshal.FreeCoTaskMem(textPtr);
            Marshal.FreeCoTaskMem(fontPtr);
        }
    }

    public unsafe void UpdateOverlayLine(OcctOverlay overlay, OcctPoint3d start, OcctPoint3d end)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Line);
        OcctGuard.Finite(start, nameof(start));
        OcctGuard.Finite(end, nameof(end));
        var points = new[] { start, end };
        fixed (OcctPoint3d* pointPtr = points)
        {
            var options = LineOptions(
                NativeOverlayLineUpdateMask.Geometry,
                OcctOverlayPrimitiveType.Line,
                (IntPtr)pointPtr,
                points.Length);
            CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_line_update(
                _handle,
                overlay.Id,
                in options));
        }
    }

    public unsafe void UpdateOverlayPolyline(OcctOverlay overlay, IReadOnlyList<OcctPoint3d> points)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Polyline);
        var values = ValidateOverlayPoints(points, nameof(points));
        fixed (OcctPoint3d* pointPtr = values)
        {
            var options = LineOptions(
                NativeOverlayLineUpdateMask.Geometry,
                OcctOverlayPrimitiveType.Polyline,
                (IntPtr)pointPtr,
                values.Length);
            CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_line_update(
                _handle,
                overlay.Id,
                in options));
        }
    }

    public void UpdateOverlayMarker(OcctOverlay overlay, OcctPoint3d position)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Marker);
        OcctGuard.Finite(position, nameof(position));
        var options = MarkerOptions(NativeOverlayMarkerUpdateMask.Position, position);
        CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_marker_update(
            _handle,
            overlay.Id,
            in options));
    }

    public void UpdateOverlayText(OcctOverlay overlay, string text, OcctPoint3d position)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Text);
        OcctGuard.Finite(position, nameof(position));
        var textPtr = Marshal.StringToCoTaskMemUTF8(text ?? string.Empty);
        try
        {
            var options = TextOptions(
                NativeOverlayTextUpdateMask.Content | NativeOverlayTextUpdateMask.Position,
                textPtr,
                position);
            CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_text_update(
                _handle,
                overlay.Id,
                in options));
        }
        finally
        {
            Marshal.FreeCoTaskMem(textPtr);
        }
    }

    public void SetOverlayLineStyle(OcctOverlay overlay, OcctOverlayLineStyle style)
    {
        EnsureOverlay(overlay);
        if (overlay.PrimitiveType is not OcctOverlayPrimitiveType.Line and not OcctOverlayPrimitiveType.Polyline)
            throw new ArgumentException("Overlay is not a line primitive.", nameof(overlay));
        ValidateOverlayLineStyle(style);
        var options = LineOptions(
            NativeOverlayLineUpdateMask.Style,
            overlay.PrimitiveType,
            IntPtr.Zero,
            0,
            style);
        CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_line_update(
            _handle,
            overlay.Id,
            in options));
    }

    public void SetOverlayMarkerStyle(OcctOverlay overlay, OcctOverlayMarkerStyle style)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Marker);
        ValidateOverlayMarkerStyle(style);
        var options = MarkerOptions(NativeOverlayMarkerUpdateMask.Style, default, style);
        CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_marker_update(
            _handle,
            overlay.Id,
            in options));
    }

    public void SetOverlayTextStyle(OcctOverlay overlay, OcctOverlayTextStyle style)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Text);
        ValidateOverlayTextStyle(style);
        var fontPtr = Marshal.StringToCoTaskMemUTF8(style.FontName);
        try
        {
            var options = TextOptions(
                NativeOverlayTextUpdateMask.Style,
                IntPtr.Zero,
                default,
                style,
                fontPtr);
            CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_text_update(
                _handle,
                overlay.Id,
                in options));
        }
        finally
        {
            Marshal.FreeCoTaskMem(fontPtr);
        }
    }

    private OcctOverlayPrimitiveType GetOverlayPrimitiveType(long id)
    {
        EnsureNotDisposed();
        CheckOverlayStatus(OverlayNativeMethods.occt_engine_overlay_primitive_type_get(
            _handle,
            id,
            out var value));
        if (!Enum.IsDefined(typeof(OcctOverlayPrimitiveType), value))
            throw new InvalidOperationException($"Native overlay primitive type {value} is not supported.");
        return (OcctOverlayPrimitiveType)value;
    }

    private static NativeOverlayLineOptions LineOptions(
        NativeOverlayLineUpdateMask updateMask,
        OcctOverlayPrimitiveType primitiveType,
        IntPtr points,
        int pointCount,
        OcctOverlayLineStyle? style = null)
    {
        var actual = style ?? new OcctOverlayLineStyle();
        return new NativeOverlayLineOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeOverlayLineOptions>(),
            ApiVersion = 1,
            UpdateMask = updateMask,
            PrimitiveType = (int)primitiveType,
            Points = points,
            PointCount = pointCount,
            Pattern = (int)actual.Pattern,
            Width = actual.Width,
            Red = actual.Color.R / 255.0,
            Green = actual.Color.G / 255.0,
            Blue = actual.Color.B / 255.0
        };
    }

    private static NativeOverlayMarkerOptions MarkerOptions(
        NativeOverlayMarkerUpdateMask updateMask,
        OcctPoint3d position,
        OcctOverlayMarkerStyle? style = null)
    {
        var actual = style ?? new OcctOverlayMarkerStyle();
        return new NativeOverlayMarkerOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeOverlayMarkerOptions>(),
            ApiVersion = 1,
            UpdateMask = updateMask,
            Position = position,
            Marker = (int)actual.Marker,
            Scale = actual.Scale,
            Red = actual.Color.R / 255.0,
            Green = actual.Color.G / 255.0,
            Blue = actual.Color.B / 255.0
        };
    }

    private static NativeOverlayTextOptions TextOptions(
        NativeOverlayTextUpdateMask updateMask,
        IntPtr text,
        OcctPoint3d position,
        OcctOverlayTextStyle? style = null,
        IntPtr fontName = default)
    {
        var actual = style ?? new OcctOverlayTextStyle();
        return new NativeOverlayTextOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeOverlayTextOptions>(),
            ApiVersion = 1,
            UpdateMask = updateMask,
            Text = text,
            Position = position,
            Height = actual.Height,
            Red = actual.Color.R / 255.0,
            Green = actual.Color.G / 255.0,
            Blue = actual.Color.B / 255.0,
            Zoomable = actual.Zoomable ? 1 : 0,
            FontName = fontName
        };
    }

    private void CheckOverlayStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }

    private static OcctPoint3d[] ValidateOverlayPoints(IReadOnlyList<OcctPoint3d> points, string name)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count < 2) throw new ArgumentException("Overlay polyline requires at least two points.", name);
        var values = new OcctPoint3d[points.Count];
        for (var index = 0; index < points.Count; index++)
        {
            OcctGuard.Finite(points[index], name);
            values[index] = points[index];
        }
        return values;
    }

    private static void ValidateOverlayLineStyle(OcctOverlayLineStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        if (!Enum.IsDefined(style.Pattern)) throw new ArgumentOutOfRangeException(nameof(style.Pattern));
        OcctGuard.Positive(style.Width, nameof(style.Width));
    }

    private static void ValidateOverlayMarkerStyle(OcctOverlayMarkerStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        if (!Enum.IsDefined(style.Marker)) throw new ArgumentOutOfRangeException(nameof(style.Marker));
        OcctGuard.Positive(style.Scale, nameof(style.Scale));
    }

    private static void ValidateOverlayTextStyle(OcctOverlayTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        OcctGuard.Positive(style.Height, nameof(style.Height));
        ArgumentException.ThrowIfNullOrWhiteSpace(style.FontName);
    }
}
