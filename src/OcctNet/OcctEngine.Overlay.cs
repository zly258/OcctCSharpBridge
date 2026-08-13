namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctOverlay AddOverlayLine(OcctPoint3d start, OcctPoint3d end, OcctOverlayLineStyle? style = null)
    {
        OcctGuard.Finite(start, nameof(start));
        OcctGuard.Finite(end, nameof(end));
        var actual = style ?? new OcctOverlayLineStyle();
        ValidateOverlayLineStyle(actual);
        EnsureInitialized();
        return CheckOverlay(OverlayNativeMethods.occt_add_overlay_line(_handle, start, end, (int)actual.Pattern, actual.Width, actual.Color.R / 255.0, actual.Color.G / 255.0, actual.Color.B / 255.0), OcctOverlayPrimitiveType.Line);
    }

    public OcctOverlay AddOverlayPolyline(IReadOnlyList<OcctPoint3d> points, OcctOverlayLineStyle? style = null)
    {
        var values = ValidateOverlayPoints(points, nameof(points));
        var actual = style ?? new OcctOverlayLineStyle();
        ValidateOverlayLineStyle(actual);
        EnsureInitialized();
        return CheckOverlay(OverlayNativeMethods.occt_add_overlay_polyline(_handle, values, values.Length, (int)actual.Pattern, actual.Width, actual.Color.R / 255.0, actual.Color.G / 255.0, actual.Color.B / 255.0), OcctOverlayPrimitiveType.Polyline);
    }

    public OcctOverlay AddOverlayMarker(OcctPoint3d position, OcctOverlayMarkerStyle? style = null)
    {
        OcctGuard.Finite(position, nameof(position));
        var actual = style ?? new OcctOverlayMarkerStyle();
        ValidateOverlayMarkerStyle(actual);
        EnsureInitialized();
        return CheckOverlay(OverlayNativeMethods.occt_add_overlay_marker(_handle, position, (int)actual.Marker, actual.Scale, actual.Color.R / 255.0, actual.Color.G / 255.0, actual.Color.B / 255.0), OcctOverlayPrimitiveType.Marker);
    }

    public OcctOverlay AddOverlayText(string text, OcctPoint3d position, OcctOverlayTextStyle? style = null)
    {
        OcctGuard.Finite(position, nameof(position));
        var actual = style ?? new OcctOverlayTextStyle();
        ValidateOverlayTextStyle(actual);
        EnsureInitialized();
        return CheckOverlay(OverlayNativeMethods.occt_add_overlay_text(_handle, text ?? string.Empty, position, actual.Height, actual.Color.R / 255.0, actual.Color.G / 255.0, actual.Color.B / 255.0, actual.Zoomable ? 1 : 0, actual.FontName), OcctOverlayPrimitiveType.Text);
    }

    public void UpdateOverlayLine(OcctOverlay overlay, OcctPoint3d start, OcctPoint3d end)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Line);
        OcctGuard.Finite(start, nameof(start));
        OcctGuard.Finite(end, nameof(end));
        CheckInitialized(() => OverlayNativeMethods.occt_update_overlay_line(_handle, overlay.Id, start, end));
    }

    public void UpdateOverlayPolyline(OcctOverlay overlay, IReadOnlyList<OcctPoint3d> points)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Polyline);
        var values = ValidateOverlayPoints(points, nameof(points));
        CheckInitialized(() => OverlayNativeMethods.occt_update_overlay_polyline(_handle, overlay.Id, values, values.Length));
    }

    public void UpdateOverlayMarker(OcctOverlay overlay, OcctPoint3d position)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Marker);
        OcctGuard.Finite(position, nameof(position));
        CheckInitialized(() => OverlayNativeMethods.occt_update_overlay_marker(_handle, overlay.Id, position));
    }

    public void UpdateOverlayText(OcctOverlay overlay, string text, OcctPoint3d position)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Text);
        OcctGuard.Finite(position, nameof(position));
        CheckInitialized(() => OverlayNativeMethods.occt_update_overlay_text(_handle, overlay.Id, text ?? string.Empty, position));
    }

    public void SetOverlayLineStyle(OcctOverlay overlay, OcctOverlayLineStyle style)
    {
        EnsureOverlay(overlay);
        if (overlay.PrimitiveType is not OcctOverlayPrimitiveType.Line and not OcctOverlayPrimitiveType.Polyline)
            throw new ArgumentException("Overlay is not a line primitive.", nameof(overlay));
        ValidateOverlayLineStyle(style);
        CheckInitialized(() => OverlayNativeMethods.occt_set_overlay_line_style(_handle, overlay.Id, (int)style.Pattern, style.Width, style.Color.R / 255.0, style.Color.G / 255.0, style.Color.B / 255.0));
    }

    public void SetOverlayMarkerStyle(OcctOverlay overlay, OcctOverlayMarkerStyle style)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Marker);
        ValidateOverlayMarkerStyle(style);
        CheckInitialized(() => OverlayNativeMethods.occt_set_overlay_marker_style(_handle, overlay.Id, (int)style.Marker, style.Scale, style.Color.R / 255.0, style.Color.G / 255.0, style.Color.B / 255.0));
    }

    public void SetOverlayTextStyle(OcctOverlay overlay, OcctOverlayTextStyle style)
    {
        EnsureOverlay(overlay, OcctOverlayPrimitiveType.Text);
        ValidateOverlayTextStyle(style);
        CheckInitialized(() => OverlayNativeMethods.occt_set_overlay_text_style(_handle, overlay.Id, style.Height, style.Color.R / 255.0, style.Color.G / 255.0, style.Color.B / 255.0, style.Zoomable ? 1 : 0, style.FontName));
    }

    private OcctOverlayPrimitiveType GetOverlayPrimitiveType(long id)
    {
        EnsureNotDisposed();
        Check(OverlayNativeMethods.occt_get_overlay_primitive_type(_handle, id, out var value));
        if (!Enum.IsDefined(typeof(OcctOverlayPrimitiveType), value))
            throw new InvalidOperationException($"Native overlay primitive type {value} is not supported.");
        return (OcctOverlayPrimitiveType)value;
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
