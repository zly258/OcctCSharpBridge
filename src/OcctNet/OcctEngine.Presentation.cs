namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetObjectClipPlanes(IOcctObject value, IReadOnlyList<OcctViewClipPlane> planes)
    {
        EnsureObject(value);
        ArgumentNullException.ThrowIfNull(planes);
        if (planes.Count > ViewClipPlaneLimit)
            throw new ArgumentException("Clip plane count exceeds the current view limit.", nameof(planes));

        var native = new NativeOcctViewClipPlane[planes.Count];
        for (var index = 0; index < planes.Count; index++)
        {
            var plane = planes[index] ?? throw new ArgumentException("Clip plane entries must not be null.", nameof(planes));
            OcctGuard.Finite(plane.Point, nameof(planes));
            OcctGuard.NonZero(plane.Normal, nameof(planes));
            native[index] = new NativeOcctViewClipPlane
            {
                Point = plane.Point,
                Normal = plane.Normal,
                Enabled = plane.Enabled ? 1 : 0,
                Capping = plane.Capping ? 1 : 0,
                CappingR = plane.CappingColor.R / 255.0,
                CappingG = plane.CappingColor.G / 255.0,
                CappingB = plane.CappingColor.B / 255.0
            };
        }

        CheckInitialized(() => PresentationNativeMethods.occt_set_object_clip_planes(
            _handle,
            value.Id,
            native,
            native.Length));
    }

    public void ClearObjectClipPlanes(IOcctObject value) =>
        SetObjectClipPlanes(value, Array.Empty<OcctViewClipPlane>());

    public void SetHighlightStyle(OcctHighlightStyleKind kind, OcctHighlightStyle style)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        var native = ToNativeHighlightStyle(style);
        CheckInitialized(() => PresentationNativeMethods.occt_set_global_highlight_style(_handle, (int)kind, in native));
    }

    public void SetObjectHighlightStyle(IOcctObject value, bool dynamic, OcctHighlightStyle style)
    {
        EnsureObject(value);
        var native = ToNativeHighlightStyle(style);
        CheckInitialized(() => PresentationNativeMethods.occt_set_object_highlight_style(
            _handle,
            value.Id,
            dynamic ? 1 : 0,
            in native));
    }

    public void ClearObjectHighlightStyle(IOcctObject value, bool dynamic)
    {
        EnsureObject(value);
        CheckInitialized(() => PresentationNativeMethods.occt_clear_object_highlight_style(
            _handle,
            value.Id,
            dynamic ? 1 : 0));
    }

    public void ResetDisplayMode(IOcctObject value)
    {
        EnsureObject(value);
        Check(PresentationNativeMethods.occt_reset_object_display_mode(_handle, value.Id));
    }

    public OcctDisplayMode? GetDisplayModeOverride(IOcctObject value)
    {
        EnsureObject(value);
        Check(PresentationNativeMethods.occt_get_object_display_mode(
            _handle,
            value.Id,
            out var hasOverride,
            out var displayMode));
        if (hasOverride == 0) return null;
        if (!Enum.IsDefined(typeof(OcctDisplayMode), displayMode))
            throw new InvalidOperationException($"Native object display mode {displayMode} is not supported by the managed bridge.");
        return (OcctDisplayMode)displayMode;
    }

    public void SetAutoHighlight(IOcctObject value, bool enabled)
    {
        EnsureObject(value);
        Check(PresentationNativeMethods.occt_set_object_auto_highlight(
            _handle,
            value.Id,
            enabled ? 1 : 0));
    }

    public bool GetAutoHighlight(IOcctObject value)
    {
        EnsureObject(value);
        Check(PresentationNativeMethods.occt_get_object_auto_highlight(_handle, value.Id, out var enabled));
        return enabled != 0;
    }

    public void SetInfiniteState(IOcctObject value, bool infinite)
    {
        EnsureObject(value);
        Check(PresentationNativeMethods.occt_set_object_infinite_state(
            _handle,
            value.Id,
            infinite ? 1 : 0));
    }

    public bool GetInfiniteState(IOcctObject value)
    {
        EnsureObject(value);
        Check(PresentationNativeMethods.occt_get_object_infinite_state(_handle, value.Id, out var infinite));
        return infinite != 0;
    }

    private static NativeOcctHighlightStyleSettings ToNativeHighlightStyle(OcctHighlightStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        if (!double.IsFinite(style.Transparency) || style.Transparency < 0.0 || style.Transparency > 1.0)
            throw new ArgumentOutOfRangeException(nameof(style.Transparency));
        OcctGuard.Positive(style.LineWidth, nameof(style.LineWidth));
        if (style.DisplayMode is { } displayMode && !Enum.IsDefined(displayMode))
            throw new ArgumentOutOfRangeException(nameof(style.DisplayMode));
        if (style.ZLayer is { } layer && !Enum.IsDefined(layer))
            throw new ArgumentOutOfRangeException(nameof(style.ZLayer));

        return new NativeOcctHighlightStyleSettings
        {
            R = style.Color.R / 255.0,
            G = style.Color.G / 255.0,
            B = style.Color.B / 255.0,
            Transparency = style.Transparency,
            LineWidth = style.LineWidth,
            DisplayMode = style.DisplayMode is { } mode ? (int)mode : -1,
            ZLayer = style.ZLayer is { } zLayer ? (int)zLayer : -1
        };
    }
}
